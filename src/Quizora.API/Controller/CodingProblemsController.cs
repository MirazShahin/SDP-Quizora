using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Quizora.Application.Common;
using Quizora.Application.DTOs.Coding;
using Quizora.Domain.Entities;
using Quizora.Infrastructure.Persistence;

namespace Quizora.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Company")]
public class CodingProblemsController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public CodingProblemsController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> List()
    {
        var list = await _db.CodingProblems
            .AsNoTracking()
            .OrderBy(p => p.Order).ThenBy(p => p.Title)
            .Select(p => new CodingProblemDto
            {
                Id = p.Id,
                Title = p.Title,
                Statement = p.Statement,
                Difficulty = p.Difficulty,
                TimeLimitMs = p.TimeLimitMs,
                IsActive = p.IsActive,
                Order = p.Order,
                TestCaseCount = p.TestCases.Count
            })
            .ToListAsync();

        return Ok(Result<List<CodingProblemDto>>.Success(list));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var p = await _db.CodingProblems
            .Include(x => x.TestCases)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (p == null)
            return Ok(Result<CodingProblemDto>.Failure("Not found"));

        return Ok(Result<CodingProblemDto>.Success(Map(p)));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCodingProblemDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title) || string.IsNullOrWhiteSpace(dto.Statement))
            return Ok(Result.Failure("Title and statement required"));

        if (dto.TestCases == null || dto.TestCases.Count == 0)
            return Ok(Result.Failure("At least one test case required"));

        var maxOrder = await _db.CodingProblems.MaxAsync(p => (int?)p.Order) ?? 0;

        var entity = new CodingProblem
        {
            Title = dto.Title.Trim(),
            Statement = dto.Statement.Trim(),
            Difficulty = string.IsNullOrWhiteSpace(dto.Difficulty) ? "Easy" : dto.Difficulty.Trim(),
            TimeLimitMs = dto.TimeLimitMs <= 0 ? 3000 : dto.TimeLimitMs,
            IsActive = true,
            Order = maxOrder + 1,
            TestCases = dto.TestCases.Select((t, i) => new CodingTestCase
            {
                Input = t.Input ?? "",
                ExpectedOutput = t.ExpectedOutput ?? "",
                IsSample = t.IsSample,
                Order = t.Order > 0 ? t.Order : i + 1
            }).ToList()
        };

        _db.CodingProblems.Add(entity);
        await _db.SaveChangesAsync();
        return Ok(Result<Guid>.Success(entity.Id, "Created"));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCodingProblemDto dto)
    {
        var entity = await _db.CodingProblems
            .Include(x => x.TestCases)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity == null)
            return Ok(Result.Failure("Not found"));

        if (string.IsNullOrWhiteSpace(dto.Title) || string.IsNullOrWhiteSpace(dto.Statement))
            return Ok(Result.Failure("Title and statement required"));

        if (dto.TestCases == null || dto.TestCases.Count == 0)
            return Ok(Result.Failure("At least one test case required"));

        entity.Title = dto.Title.Trim();
        entity.Statement = dto.Statement.Trim();
        entity.Difficulty = string.IsNullOrWhiteSpace(dto.Difficulty) ? "Easy" : dto.Difficulty.Trim();
        entity.TimeLimitMs = dto.TimeLimitMs <= 0 ? 3000 : dto.TimeLimitMs;
        entity.IsActive = dto.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        _db.CodingTestCases.RemoveRange(entity.TestCases);
        entity.TestCases = dto.TestCases
            .Select((t, i) => new CodingTestCase
            {
                Input = t.Input ?? "",
                ExpectedOutput = t.ExpectedOutput ?? "",
                IsSample = t.IsSample,
                Order = t.Order > 0 ? t.Order : i + 1
            }).ToList();

        await _db.SaveChangesAsync();
        return Ok(Result.Success("Updated"));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var entity = await _db.CodingProblems.FindAsync(id);
        if (entity == null)
            return Ok(Result.Failure("Not found"));

        _db.CodingProblems.Remove(entity);
        await _db.SaveChangesAsync();
        return Ok(Result.Success("Deleted"));
    }

    private static CodingProblemDto Map(CodingProblem p) => new()
    {
        Id = p.Id,
        Title = p.Title,
        Statement = p.Statement,
        Difficulty = p.Difficulty,
        TimeLimitMs = p.TimeLimitMs,
        IsActive = p.IsActive,
        Order = p.Order,
        TestCaseCount = p.TestCases.Count,
        TestCases = p.TestCases.OrderBy(t => t.Order).Select(t => new CodingTestCaseDto
        {
            Id = t.Id,
            Input = t.Input,
            ExpectedOutput = t.ExpectedOutput,
            IsSample = t.IsSample,
            Order = t.Order
        }).ToList()
    };
}
