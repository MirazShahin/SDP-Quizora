using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quizora.Application.Common;
using Quizora.Application.DTOs.Questions;
using Quizora.Application.DTOs.Tests;
using Quizora.Application.Interfaces;
using Quizora.Domain.Entities;
using Quizora.Domain.Enums;
using Quizora.Infrastructure.Repositories;
using System.Security.Claims;

namespace Quizora.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Company")]
public class TestsController : ControllerBase
{
    private readonly ITestService _testService;
    private readonly ITestRepository _testRepository;
    private readonly IUserRepository _userRepository;

    public TestsController(
    ITestService testService,
    ITestRepository testRepository,
    IUserRepository userRepository)
    {
        _testService = testService;
        _testRepository = testRepository;
        _userRepository = userRepository;
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<Result<List<TestDto>>>> GetMyTests()
    {
        var result = await _testService.GetMyTestsAsync(GetUserId());

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<Result<TestDto>>> CreateTest(CreateTestDto dto)
    {
        var result = await _testService.CreateTestAsync(GetUserId(), dto);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("{testId}/questions")]
    public async Task<ActionResult<Result>> AddQuestion(Guid testId, [FromBody] CreateQuestionDto dto)
    {
        var test = await _testRepository.GetByIdAsync(testId);
        if (test == null)
            return Result.Failure("Test not found");

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _userRepository.GetByIdAsync(userId);

        if (user?.Company?.Id != test.CompanyId)
            return Result.Failure("Unauthorized");

        var questionType = string.IsNullOrWhiteSpace(dto.QuestionType) ? "MCQ" : dto.QuestionType;

        if (questionType == "ShortAnswer" || questionType == "OneAnswer")
        {
            var question = new Question
            {
                TestId = testId,
                Text = dto.Text,
                QuestionType = "ShortAnswer",
                Order = 1
            };

            await _testRepository.AddQuestionAsync(question);
            return Result.Success("Short answer question added successfully");
        }

        // MCQ validation
        if (dto.Options == null || dto.Options.Count != 4)
            return Result.Failure("Exactly 4 options required for MCQ");

        if (dto.Options.Count(o => o.IsCorrect) != 1)
            return Result.Failure("Exactly one correct option required");

        var mcq = new Question
        {
            TestId = testId,
            Text = dto.Text,
            QuestionType = "MCQ",
            Order = 1,
            Options = dto.Options.Select(o => new Option
            {
                Text = o.Text,
                IsCorrect = o.IsCorrect
            }).ToList()
        };

        await _testRepository.AddQuestionAsync(mcq);
        return Result.Success("MCQ question added successfully");
    }

    [HttpPut("{testId}/status")]
    public async Task<ActionResult<Result>> UpdateStatus(Guid testId, [FromBody] TestStatus status)
    {
        var result = await _testService.UpdateStatusAsync(GetUserId(), testId, status);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }
}