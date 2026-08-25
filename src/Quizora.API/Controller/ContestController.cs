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

    // ========== PUBLIC: List all public contests (everyone) ==========
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<Result<List<ContestListItemDto>>>> GetPublicContests()
    {
        var now = DateTime.UtcNow;

        var contests = await _db.Tests
            .AsNoTracking()
            .Include(t => t.Company)
            .Include(t => t.CodingProblems)
            .Where(t => t.IsContest && t.IsPublic && t.Status != TestStatus.Draft)
            .OrderByDescending(t => t.ContestStartAt ?? t.CreatedAt)
            .Select(t => new ContestListItemDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                DurationInMinutes = t.DurationInMinutes,
                ContestStartAt = t.ContestStartAt,
                ContestEndAt = t.ContestEndAt,
                ProblemCount = t.CodingProblems.Count,
                CompanyName = t.Company != null ? t.Company.CompanyName : "",
                Status = ComputeStatus(t.ContestStartAt, t.ContestEndAt, t.Status, now)
            })
            .ToListAsync();

        return Ok(Result<List<ContestListItemDto>>.Success(contests));
    }

    // ========== PUBLIC: Contest details ==========
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

    // ========== COMPANY: Create Contest ==========
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

        // Validate problems exist
        var problems = await _db.CodingProblems
            .Where(p => dto.CodingProblemIds.Contains(p.Id) && p.IsActive)
            .ToListAsync();

        if (problems.Count != dto.CodingProblemIds.Count)
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
            _db.Set<TestCodingProblem>().Add(new TestCodingProblem
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

        // Return detail
        return await GetContest(test.Id);
    }

    // ========== COMPANY: My Contests ==========
    [HttpGet("my")]
    [Authorize(Roles = "Company")]
    public async Task<ActionResult<Result<List<ContestListItemDto>>>> GetMyContests()
    {
        var userId = GetUserId();
        var user = await _db.Users.Include(u => u.Company).FirstOrDefaultAsync(u => u.Id == userId);
        if (user?.Company == null)
            return Ok(Result<List<ContestListItemDto>>.Failure("Company not found"));

        var now = DateTime.UtcNow;

        var list = await _db.Tests
            .AsNoTracking()
            .Include(t => t.CodingProblems)
            .Where(t => t.IsContest && t.CompanyId == user.Company.Id)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new ContestListItemDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                DurationInMinutes = t.DurationInMinutes,
                ContestStartAt = t.ContestStartAt,
                ContestEndAt = t.ContestEndAt,
                ProblemCount = t.CodingProblems.Count,
                CompanyName = user.Company.CompanyName,
                Status = ComputeStatus(t.ContestStartAt, t.ContestEndAt, t.Status, now)
            })
            .ToListAsync();

        return Ok(Result<List<ContestListItemDto>>.Success(list));
    }

    private static string ComputeStatus(DateTime? start, DateTime? end, TestStatus status, DateTime now)
    {
        if (status == TestStatus.Draft) return "Draft";
        if (start.HasValue && now < start.Value) return "Upcoming";
        if (end.HasValue && now > end.Value) return "Ended";
        if (start.HasValue && end.HasValue && now >= start.Value && now <= end.Value) return "Running";
        if (!start.HasValue && !end.HasValue) return "Open";
        return "Open";
    }
}
