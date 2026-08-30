using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Quizora.Application.Common;
using Quizora.Application.DTOs.Contest;
using Quizora.Domain.Entities;
using Quizora.Domain.Enums;
using Quizora.Infrastructure.Persistence;
using System.Security.Claims;

namespace Quizora.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContestsController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public ContestsController(ApplicationDbContext db) => _db = db;

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<Result<List<ContestListItemDto>>>> GetPublicContests()
    {
        var now = DateTime.UtcNow;
        var rows = await _db.Tests
            .AsNoTracking()
            .Where(t => t.IsContest && t.IsPublic && t.Status != TestStatus.Draft)
            .OrderByDescending(t => t.ContestStartAt ?? t.CreatedAt)
            .Select(t => new
            {
                t.Id,
                t.Title,
                t.Description,
                t.DurationInMinutes,
                t.ContestStartAt,
                t.ContestEndAt,
                t.Status,
                ProblemCount = t.CodingProblems.Count,
                CompanyName = t.Company != null ? t.Company.CompanyName : ""
            })
            .ToListAsync();

        var contests = rows.Select(t => new ContestListItemDto
        {
            Id = t.Id,
            Title = t.Title,
            Description = t.Description,
            DurationInMinutes = t.DurationInMinutes,
            ContestStartAt = t.ContestStartAt,
            ContestEndAt = t.ContestEndAt,
            ProblemCount = t.ProblemCount,
            CompanyName = t.CompanyName,
            Status = ComputeStatus(t.ContestStartAt, t.ContestEndAt, t.Status, now)
        }).ToList();

        return Ok(Result<List<ContestListItemDto>>.Success(contests));
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<Result<ContestDetailDto>>> GetContest(Guid id)
    {
        var now = DateTime.UtcNow;
        var test = await _db.Tests
            .AsNoTracking()
            .Include(t => t.CodingProblems)
                .ThenInclude(tcp => tcp.Problem)
            .FirstOrDefaultAsync(t => t.Id == id && t.IsContest);

        if (test == null)
            return Ok(Result<ContestDetailDto>.Failure("Contest not found"));

        if (!test.IsPublic && test.Status == TestStatus.Draft)
            return Ok(Result<ContestDetailDto>.Failure("Contest not available"));

        var letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        var problems = test.CodingProblems
            .OrderBy(x => x.Order)
            .Select((x, i) => new ContestProblemItemDto
            {
                CodingProblemId = x.CodingProblemId,
                Letter = i < letters.Length ? letters[i].ToString() : (i + 1).ToString(),
                Title = x.Problem?.Title ?? "",
                Difficulty = x.Problem?.Difficulty ?? "Easy",
                Points = x.Points,
                Order = x.Order,
                TimeLimitMs = x.Problem?.TimeLimitMs ?? 3000
            })
            .ToList();

        var dto = new ContestDetailDto
        {
            Id = test.Id,
            Title = test.Title,
            Description = test.Description,
            DurationInMinutes = test.DurationInMinutes,
            ContestStartAt = test.ContestStartAt,
            ContestEndAt = test.ContestEndAt,
            IsPublic = test.IsPublic,
            Status = ComputeStatus(test.ContestStartAt, test.ContestEndAt, test.Status, now),
            Problems = problems
        };

        return Ok(Result<ContestDetailDto>.Success(dto));
    }

    /// <summary>
    /// ICPC-style standings for a contest.
    /// Rank by solved desc, then penalty asc.
    /// Penalty = minutes to first AC + 20 * wrong tries before AC (CE ignored).
    /// </summary>
    [HttpGet("{id:guid}/standings")]
    [AllowAnonymous]
    public async Task<IActionResult> GetStandings(Guid id)
    {
        try
        {
            var test = await _db.Tests
                .AsNoTracking()
                .Include(t => t.CodingProblems)
                .FirstOrDefaultAsync(t => t.Id == id && t.IsContest);

            if (test == null)
                return Ok(Result<object>.Failure("Contest not found"));

            var problemRows = test.CodingProblems.OrderBy(x => x.Order).ToList();
            var problemIds = problemRows.Select(x => x.CodingProblemId).ToList();
            var letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            var problemMeta = problemRows.Select((x, i) => new
            {
                x.CodingProblemId,
                Letter = i < letters.Length ? letters[i].ToString() : (i + 1).ToString()
            }).ToList();

            var contestStart = test.ContestStartAt ?? test.CreatedAt;

            // Submissions in this contest only
            var subs = await _db.CodingSubmissions
                .AsNoTracking()
                .Include(s => s.User)
                .Where(s => s.ContestId == id && problemIds.Contains(s.CodingProblemId))
                .OrderBy(s => s.CreatedAt)
                .ToListAsync();

            // Fallback if ContestId not backfilled yet: all subs on contest problems (optional — comment out if noisy)
            if (subs.Count == 0)
            {
                subs = await _db.CodingSubmissions
                    .AsNoTracking()
                    .Include(s => s.User)
                    .Where(s => problemIds.Contains(s.CodingProblemId))
                    .OrderBy(s => s.CreatedAt)
                    .ToListAsync();
            }

            var standings = new List<object>();

            foreach (var g in subs.GroupBy(s => s.UserId))
            {
                int solved = 0;
                int penalty = 0;
                var cells = new List<object>();

                foreach (var pm in problemMeta)
                {
                    var list = g.Where(s => s.CodingProblemId == pm.CodingProblemId)
                                .OrderBy(s => s.CreatedAt)
                                .ToList();

                    var ac = list.FirstOrDefault(s => IsAccepted(s.Verdict));
                    // wrong tries before AC (skip pure CE if you want)
                    var wrongBefore = 0;
                    foreach (var s in list)
                    {
                        if (IsAccepted(s.Verdict)) break;
                        if (IsCompileError(s.Verdict)) continue;
                        wrongBefore++;
                    }

                    if (ac != null)
                    {
                        solved++;
                        var minutes = (int)Math.Max(0, (ac.CreatedAt - contestStart).TotalMinutes);
                        penalty += minutes + wrongBefore * 20;
                        cells.Add(new
                        {
                            Letter = pm.Letter,
                            Status = wrongBefore > 0 ? $"+{wrongBefore}" : "✓",
                            IsAccepted = true
                        });
                    }
                    else if (list.Count > 0)
                    {
                        cells.Add(new
                        {
                            Letter = pm.Letter,
                            Status = $"-{list.Count}",
                            IsAccepted = false
                        });
                    }
                    else
                    {
                        cells.Add(new
                        {
                            Letter = pm.Letter,
                            Status = "",
                            IsAccepted = false
                        });
                    }
                }

                var user = g.First().User;
                standings.Add(new
                {
                    UserId = g.Key,
                    Name = user?.FullName ?? "User",
                    Solved = solved,
                    Penalty = penalty,
                    Cells = cells
                });
            }

            var ranked = standings
                .OrderByDescending(r => ((dynamic)r).Solved)
                .ThenBy(r => ((dynamic)r).Penalty)
                .Select((r, idx) => new
                {
                    Rank = idx + 1,
                    ((dynamic)r).UserId,
                    ((dynamic)r).Name,
                    ((dynamic)r).Solved,
                    ((dynamic)r).Penalty,
                    ((dynamic)r).Cells
                })
                .ToList();

            return Ok(Result<object>.Success(new
            {
                ContestId = id,
                Title = test.Title,
                Problems = problemMeta,
                Standings = ranked
            }));
        }
        catch (Exception ex)
        {
            return Ok(Result<object>.Failure(ex.InnerException?.Message ?? ex.Message));
        }
    }

    [HttpGet("{id:guid}/my-submissions")]
    [Authorize]
    public async Task<IActionResult> MySubmissions(Guid id)
    {
        try
        {
            var userId = GetUserId();
            var list = await _db.CodingSubmissions
                .AsNoTracking()
                .Include(s => s.Problem)
                .Where(s => s.ContestId == id && s.UserId == userId)
                .OrderByDescending(s => s.CreatedAt)
                .Take(50)
                .Select(s => new
                {
                    s.Id,
                    s.CodingProblemId,
                    ProblemTitle = s.Problem != null ? s.Problem.Title : "",
                    s.Language,
                    s.Verdict,
                    s.PassedCount,
                    s.TotalCount,
                    s.CreatedAt
                })
                .ToListAsync();

            return Ok(Result<object>.Success(list));
        }
        catch (Exception ex)
        {
            return Ok(Result<object>.Failure(ex.Message));
        }
    }

    [HttpPost]
    [Authorize(Roles = "Company")]
    public async Task<ActionResult<Result<ContestDetailDto>>> CreateContest([FromBody] CreateContestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
            return Ok(Result<ContestDetailDto>.Failure("Title is required"));
        if (dto.DurationInMinutes < 15)
            return Ok(Result<ContestDetailDto>.Failure("Duration must be at least 15 minutes"));
        if (dto.CodingProblemIds == null || dto.CodingProblemIds.Count == 0)
            return Ok(Result<ContestDetailDto>.Failure("Select at least one coding problem"));

        var userId = GetUserId();
        var user = await _db.Users
            .Include(u => u.Company)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user?.Company == null)
            return Ok(Result<ContestDetailDto>.Failure("Company profile not found"));

        var distinctIds = dto.CodingProblemIds.Distinct().ToList();
        var problems = await _db.CodingProblems
            .Where(p => distinctIds.Contains(p.Id) && p.IsActive)
            .ToListAsync();

        if (problems.Count != distinctIds.Count)
            return Ok(Result<ContestDetailDto>.Failure("One or more problems not found or inactive"));

        var test = new Test
        {
            Id = Guid.NewGuid(),
            CompanyId = user.Company.Id,
            Title = dto.Title.Trim(),
            Description = dto.Description?.Trim(),
            DurationInMinutes = dto.DurationInMinutes,
            IsContest = true,
            IsPublic = true,
            Status = TestStatus.Active,
            ContestStartAt = dto.ContestStartAt?.ToUniversalTime(),
            ContestEndAt = dto.ContestEndAt?.ToUniversalTime(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Tests.Add(test);

        for (int i = 0; i < dto.CodingProblemIds.Count; i++)
        {
            var points = (dto.Points != null && i < dto.Points.Count) ? dto.Points[i] : 100;
            _db.TestCodingProblems.Add(new TestCodingProblem
            {
                Id = Guid.NewGuid(),
                TestId = test.Id,
                CodingProblemId = dto.CodingProblemIds[i],
                Order = i + 1,
                Points = points,
                CreatedAt = DateTime.UtcNow
            });
        }

        await _db.SaveChangesAsync();
        return await GetContest(test.Id);
    }

    [HttpGet("my")]
    [Authorize(Roles = "Company")]
    public async Task<ActionResult<Result<List<ContestListItemDto>>>> GetMyContests()
    {
        var userId = GetUserId();
        var user = await _db.Users.Include(u => u.Company).FirstOrDefaultAsync(u => u.Id == userId);
        if (user?.Company == null)
            return Ok(Result<List<ContestListItemDto>>.Failure("Company not found"));

        var now = DateTime.UtcNow;
        var companyId = user.Company.Id;
        var companyName = user.Company.CompanyName;

        var rows = await _db.Tests
            .AsNoTracking()
            .Where(t => t.IsContest && t.CompanyId == companyId)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new
            {
                t.Id,
                t.Title,
                t.Description,
                t.DurationInMinutes,
                t.ContestStartAt,
                t.ContestEndAt,
                t.Status,
                ProblemCount = t.CodingProblems.Count
            })
            .ToListAsync();

        var list = rows.Select(t => new ContestListItemDto
        {
            Id = t.Id,
            Title = t.Title,
            Description = t.Description,
            DurationInMinutes = t.DurationInMinutes,
            ContestStartAt = t.ContestStartAt,
            ContestEndAt = t.ContestEndAt,
            ProblemCount = t.ProblemCount,
            CompanyName = companyName,
            Status = ComputeStatus(t.ContestStartAt, t.ContestEndAt, t.Status, now)
        }).ToList();

        return Ok(Result<List<ContestListItemDto>>.Success(list));
    }

    private static string ComputeStatus(DateTime? start, DateTime? end, TestStatus status, DateTime now)
    {
        if (status == TestStatus.Draft) return "Draft";
        if (start.HasValue && now < start.Value) return "Upcoming";
        if (end.HasValue && now > end.Value) return "Ended";
        if (start.HasValue && end.HasValue && now >= start.Value && now <= end.Value) return "Running";
        return "Open";
    }

    private static bool IsAccepted(string? v)
        => !string.IsNullOrWhiteSpace(v) &&
           (v.Equals("Accepted", StringComparison.OrdinalIgnoreCase) ||
            v.Equals("AC", StringComparison.OrdinalIgnoreCase));

    private static bool IsCompileError(string? v)
        => !string.IsNullOrWhiteSpace(v) &&
           (v.Contains("Compilation", StringComparison.OrdinalIgnoreCase) ||
            v.Equals("CE", StringComparison.OrdinalIgnoreCase));
}