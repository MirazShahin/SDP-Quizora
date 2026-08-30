using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Quizora.Application.Common;
using Quizora.Application.DTOs.Code;
using Quizora.Application.Interfaces;
using Quizora.Domain.Entities;
using Quizora.Infrastructure.Persistence;

namespace Quizora.API.Controllers;

[ApiController]
[Route("api/coding")]
[Authorize(Roles = "Candidate")]
public class CodingPracticeController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly ICodeExecutionService _runner;

    public CodingPracticeController(ApplicationDbContext db, ICodeExecutionService runner)
    {
        _db = db;
        _runner = runner;
    }

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private static string Norm(string? s) =>
        (s ?? "").Replace("\r\n", "\n").TrimEnd('\r', '\n', ' ', '\t');
     
    [HttpGet("problems")]
    public async Task<IActionResult> List()
    {
        try
        {
            var problems = await _db.CodingProblems.AsNoTracking()
                .Where(p => p.IsActive)
                .OrderBy(p => p.Order).ThenBy(p => p.Title)
                .Select(p => new
                {
                    p.Id,
                    p.Title,
                    p.TimeLimitMs
                })
                .ToListAsync();
             
            Dictionary<Guid, string> statusMap = new();
            try
            {
                var userId = UserId;
                var subs = await _db.CodingSubmissions.AsNoTracking()
                    .Where(s => s.UserId == userId)
                    .Select(s => new { s.CodingProblemId, s.Verdict, s.CreatedAt })
                    .ToListAsync();

                statusMap = subs
                    .GroupBy(s => s.CodingProblemId)
                    .ToDictionary(
                        g => g.Key,
                        g => g.FirstOrDefault(x => x.Verdict == "Accepted")?.Verdict
                             ?? g.OrderByDescending(x => x.CreatedAt).First().Verdict
                    );
            }
            catch
            {
                
            }

            var result = problems.Select(p => new
            {
                p.Id,
                p.Title,
                p.TimeLimitMs,
                Status = statusMap.TryGetValue(p.Id, out var st) ? st : "—"
            }).ToList();

            return Ok(Result<object>.Success(result));
        }
        catch (Exception ex)
        {
            return Ok(Result<object>.Failure(ex.InnerException?.Message ?? ex.Message));
        }
    }
     
    [HttpGet("problems/{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var p = await _db.CodingProblems.AsNoTracking()
            .Include(x => x.TestCases)
            .FirstOrDefaultAsync(x => x.Id == id && x.IsActive);

        if (p == null)
            return Ok(Result<object>.Failure("Not found"));

        var samples = p.TestCases.Where(t => t.IsSample).OrderBy(t => t.Order)
            .Select(t => new { t.Input, t.ExpectedOutput }).ToList();

        return Ok(Result<object>.Success(new
        {
            p.Id,
            p.Title,
            p.Statement,
            p.TimeLimitMs,
            Samples = samples
        }));
    }
     
    [HttpPost("problems/{id:guid}/submit")]
    public async Task<IActionResult> Submit(Guid id, [FromBody] SubmitDto dto, CancellationToken ct)
    {
        try
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.SourceCode))
                return Ok(Result<object>.Failure("Source code required"));

            var p = await _db.CodingProblems
                .Include(x => x.TestCases)
                .FirstOrDefaultAsync(x => x.Id == id && x.IsActive, ct);

            if (p == null)
                return Ok(Result<object>.Failure("Not found"));

            var cases = p.TestCases.OrderBy(t => t.Order).ToList();
            if (cases.Count == 0)
                return Ok(Result<object>.Failure("No test cases"));

            var lang = (dto.Language ?? "cpp").Trim().ToLowerInvariant();
            var results = new List<object>();
            int passed = 0;
            long maxTime = 0;
            string? compileOut = null;
            string verdict = "Accepted";
            Guid? contestIdToSave = null;
            if (dto.ContestId.HasValue)
            {
                var contest = await _db.Tests.AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Id == dto.ContestId && t.IsContest, ct);

                if (contest != null)
                {
                    var now = DateTime.UtcNow;
                    bool running =
                        contest.ContestStartAt.HasValue && now >= contest.ContestStartAt.Value &&
                        (!contest.ContestEndAt.HasValue || now <= contest.ContestEndAt.Value);

                    bool registered = await _db.ContestRegistrations
                        .AnyAsync(r => r.ContestId == dto.ContestId && r.UserId == UserId, ct);

                    if (dto.ContestId.HasValue && running && !registered)
                        return Ok(Result.Failure("You are not registered for this contest."));
                }
            }
            foreach (var (tc, i) in cases.Select((t, i) => (t, i)))
            {
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
                    ok = Norm(run.Stdout) == Norm(tc.ExpectedOutput);
                    status = ok ? "OK" : "WA";
                    if (!ok && verdict == "Accepted") verdict = "WrongAnswer";
                }

                if (ok) passed++;
                results.Add(new
                {
                    Index = i + 1,
                    tc.IsSample,
                    Passed = ok,
                    Status = status,
                    run.TimeMs
                });

                if (verdict == "CompilationError") break;
            }

            if (verdict == "Accepted" && passed < cases.Count)
                verdict = "WrongAnswer";

            var sub = new CodingSubmission
            {
                UserId = UserId,
                CodingProblemId = id,
                ContestId = dto.ContestId,
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

            return Ok(Result<object>.Success(new
            {
                sub.Id,
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
            return Ok(Result<object>.Failure(ex.InnerException?.Message ?? ex.Message));
        }
    }

    public class SubmitDto
    {
        public string Language { get; set; } = "cpp";
        public string SourceCode { get; set; } = "";
        public Guid? ContestId { get; set; }
    }
}