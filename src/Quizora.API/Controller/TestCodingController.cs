using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Quizora.Application.Common;
using Quizora.Application.DTOs.Contest;
using Quizora.Domain.Entities;
using Quizora.Infrastructure.Persistence;

namespace Quizora.API.Controllers;

[ApiController]
[Route("api/tests/{testId:guid}/coding-problems")]
[Authorize(Roles = "Company")]
public class TestCodingController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public TestCodingController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> List(Guid testId)
    {
        var list = await _db.TestCodingProblems
            .AsNoTracking()
            .Include(x => x.Problem)
            .Where(x => x.TestId == testId)
            .OrderBy(x => x.Order)
            .Select(x => new TestCodingProblemDto
            {
                Id = x.Id,
                CodingProblemId = x.CodingProblemId,
                Letter = ((char)('A' + x.Order)).ToString(),
                Title = x.Problem.Title,
                Order = x.Order,
                Points = x.Points,
                TimeLimitMs = x.Problem.TimeLimitMs,
                TestCaseCount = x.Problem.TestCases.Count
            })
            .ToListAsync();

        return Ok(Result<List<TestCodingProblemDto>>.Success(list));
    }

    [HttpPut]
    public async Task<IActionResult> Assign(Guid testId, [FromBody] AssignCodingProblemsDto dto)
    {
        try
        {
            var test = await _db.Tests.FindAsync(testId);
            if (test == null) return Ok(Result.Failure("Test not found"));

            var ids = (dto?.CodingProblemIds ?? new()).Distinct().ToList();
            var valid = await _db.CodingProblems
                .Where(p => ids.Contains(p.Id) && p.IsActive)
                .Select(p => p.Id)
                .ToListAsync();

            if (valid.Count != ids.Count)
                return Ok(Result.Failure("Some problems not found or inactive"));

            var existing = await _db.TestCodingProblems.Where(x => x.TestId == testId).ToListAsync();
            _db.TestCodingProblems.RemoveRange(existing);
            await _db.SaveChangesAsync();

            for (int i = 0; i < ids.Count; i++)
            {
                _db.TestCodingProblems.Add(new TestCodingProblem
                {
                    TestId = testId,
                    CodingProblemId = ids[i],
                    Order = i,
                    Points = 100,
                    CreatedAt = DateTime.UtcNow
                });
            }
            await _db.SaveChangesAsync();
            return Ok(Result.Success($"Assigned {ids.Count} coding problem(s)"));
        }
        catch (Exception ex)
        {
            return Ok(Result.Failure(ex.InnerException?.Message ?? ex.Message));
        }
    }
}