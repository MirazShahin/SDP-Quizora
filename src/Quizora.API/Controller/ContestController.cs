using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Quizora.Application.Common;
using Quizora.Application.DTOs.Code;
using Quizora.Application.DTOs.Contest;
using Quizora.Application.Interfaces;
using Quizora.Domain.Entities;
using Quizora.Infrastructure.Persistence;

namespace Quizora.API.Controllers;

[ApiController]
[Route("api/contest")]
[Authorize(Roles = "Candidate")]
public class ContestController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly ICodeExecutionService _runner;

    public ContestController(ApplicationDbContext db, ICodeExecutionService runner)
    {
        _db = db;
        _runner = runner;
    }

    private Guid? GetUserId()
    {
        var s = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(s, out var id) ? id : null;
    }

    private async Task<TestInvitation?> GetOwnedInvitation(Guid invitationId)
    {
        var userId = GetUserId();
        if (userId == null) return null;

        return await _db.TestInvitations
            .Include(i => i.Test)
            .Include(i => i.Candidate)
            .FirstOrDefaultAsync(i => i.Id == invitationId && i.Candidate.UserId == userId);
    }

    private static string Letter(int order) =>
        order >= 0 && order < 26 ? ((char)('A' + order)).ToString() : (order + 1).ToString();

    [HttpGet("{invitationId:guid}")]
    public async Task<IActionResult> Overview(Guid invitationId)
    {
        var inv = await GetOwnedInvitation(invitationId);
        if (inv == null)
            return Ok(Result<ContestOverviewDto>.Failure("Invitation not found"));

        var assigned = await _db.TestCodingProblems
            .AsNoTracking()
            .Include(x => x.Problem)
            .Where(x => x.TestId == inv.TestId)
            .OrderBy(x => x.Order)
            .ToListAsync();

        var subs = await _db.CodingSubmissions
            .AsNoTracking()
            .Where(s => s.InvitationId == invitationId)
            .ToListAsync();

        var problems = assigned.Select(a =>
        {
            var my = subs.Where(s => s.CodingProblemId == a.CodingProblemId).ToList();
            var best = my.FirstOrDefault(s => s.Verdict == "Accepted")
                       ?? my.OrderByDescending(s => s.CreatedAt).FirstOrDefault();
            return new ContestProblemListItemDto
            {
                CodingProblemId = a.CodingProblemId,
                Letter = Letter(a.Order),
                Title = a.Problem.Title,
                Points = a.Points,
                TimeLimitMs = a.Problem.TimeLimitMs,
                Status = best?.Verdict ?? "—",
                SubmissionCount = my.Count
            };
        }).ToList();

        return Ok(Result<ContestOverviewDto>.Success(new ContestOverviewDto
        {
            InvitationId = inv.Id,
            TestId = inv.TestId,
            TestTitle = inv.Test.Title,
            DurationInMinutes = inv.Test.DurationInMinutes,
            Problems = problems
        }));
    }

    [HttpGet("{invitationId:guid}/problems/{problemId:guid}")]
    public async Task<IActionResult> Problem(Guid invitationId, Guid problemId)
    {
        var inv = await GetOwnedInvitation(invitationId);
        if (inv == null)
            return Ok(Result<ContestProblemDetailDto>.Failure("Invitation not found"));

        var link = await _db.TestCodingProblems
            .AsNoTracking()
            .Include(x => x.Problem).ThenInclude(p => p.TestCases)
            .FirstOrDefaultAsync(x => x.TestId == inv.TestId && x.CodingProblemId == problemId);

        if (link == null)
            return Ok(Result<ContestProblemDetailDto>.Failure("Problem not in this contest"));

        var p = link.Problem;
        var samples = p.TestCases.Where(t => t.IsSample).OrderBy(t => t.Order)
            .Select(t => new SampleTestDto { Input = t.Input, ExpectedOutput = t.ExpectedOutput }).ToList();

        return Ok(Result<ContestProblemDetailDto>.Success(new ContestProblemDetailDto
        {
            CodingProblemId = p.Id,
            Letter = Letter(link.Order),
            Title = p.Title,
            Statement = p.Statement,
            TimeLimitMs = p.TimeLimitMs,
            Points = link.Points,
            Samples = samples
        }));
    }

    [HttpPost("{invitationId:guid}/problems/{problemId:guid}/submit")]
    public async Task<IActionResult> Submit(Guid invitationId, Guid problemId,
        [FromBody] CodingSubmitRequestDto dto, CancellationToken ct)
    {
        try
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.SourceCode))
                return Ok(Result<CodingSubmitResultDto>.Failure("Source code required"));

            var inv = await GetOwnedInvitation(invitationId);
            if (inv == null)
                return Ok(Result<CodingSubmitResultDto>.Failure("Invitation not found"));

            var link = await _db.TestCodingProblems
                .AsNoTracking()
                .Include(x => x.Problem).ThenInclude(p => p.TestCases)
                .FirstOrDefaultAsync(x => x.TestId == inv.TestId && x.CodingProblemId == problemId, ct);

            if (link == null)
                return Ok(Result<CodingSubmitResultDto>.Failure("Problem not in this contest"));

            var cases = link.Problem.TestCases.OrderBy(t => t.Order).ToList();
            if (cases.Count == 0)
                return Ok(Result<CodingSubmitResultDto>.Failure("No test cases"));

            var lang = (dto.Language ?? "cpp").Trim().ToLowerInvariant();
            var results = new List<TestCaseResultDto>();
            int passed = 0;
            long maxTime = 0;
            string? compileOut = null;
            string verdict = "Accepted";

            for (int i = 0; i < cases.Count; i++)
            {
                var tc = cases[i];
                var run = await _runner.RunAsync(new CodeRunRequestDto
                {
                    Language = lang,
                    SourceCode = dto.SourceCode,
                    Stdin = tc.Input,
                    ExpectedOutput = tc.ExpectedOutput
                }, ct);

                if (!string.IsNullOrEmpty(run.CompileOutput))
                    compileOut = run.CompileOutput;

                maxTime = Math.Max(maxTime, run.TimeMs);

                string status;
                bool ok;

                if (!run.Compiled)
                {
                    status = "CE"; ok = false; verdict = "CompilationError";
                }
                else if (run.TimedOut)
                {
                    status = "TLE"; ok = false;
                    if (verdict == "Accepted") verdict = "TimeLimitExceeded";
                }
                else if (!run.Success || run.ExitCode != 0)
                {
                    status = "RE"; ok = false;
                    if (verdict == "Accepted") verdict = "RuntimeError";
                }
                else
                {
                    var actual = (run.Stdout ?? "").Replace("\r\n", "\n").TrimEnd();
                    var expected = (tc.ExpectedOutput ?? "").Replace("\r\n", "\n").TrimEnd();
                    ok = actual == expected;
                    status = ok ? "OK" : "WA";
                    if (!ok && verdict == "Accepted") verdict = "WrongAnswer";
                }

                if (ok) passed++;

                results.Add(new TestCaseResultDto
                {
                    Index = i + 1,
                    IsSample = tc.IsSample,
                    Passed = ok,
                    Status = status,
                    TimeMs = run.TimeMs,
                    Stdout = tc.IsSample ? run.Stdout : null,
                    Expected = tc.IsSample ? tc.ExpectedOutput : null
                });

                if (verdict == "CompilationError") break;
            }

            if (verdict == "Accepted" && passed < cases.Count)
                verdict = "WrongAnswer";

            var sub = new CodingSubmission
            {
                InvitationId = invitationId,
                CodingProblemId = problemId,
                Language = lang,
                SourceCode = dto.SourceCode,
                Verdict = verdict,
                PassedCount = passed,
                TotalCount = cases.Count,
                MaxTimeMs = maxTime,
                CompileOutput = compileOut,
                DetailJson = JsonSerializer.Serialize(results),
                CreatedAt = DateTime.UtcNow
            };
            _db.CodingSubmissions.Add(sub);
            await _db.SaveChangesAsync(ct);

            return Ok(Result<CodingSubmitResultDto>.Success(new CodingSubmitResultDto
            {
                SubmissionId = sub.Id,
                Verdict = verdict,
                PassedCount = passed,
                TotalCount = cases.Count,
                MaxTimeMs = maxTime,
                CompileOutput = compileOut,
                Cases = results
            }));
        }
        catch (Exception ex)
        {
            return Ok(Result<CodingSubmitResultDto>.Failure(ex.InnerException?.Message ?? ex.Message));
        }
    }
}