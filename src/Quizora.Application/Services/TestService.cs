using Quizora.Application.Common;
using Quizora.Application.DTOs.Questions;
using Quizora.Application.DTOs.Tests;
using Quizora.Application.Interfaces;
using Quizora.Domain.Entities;
using Quizora.Domain.Enums;

namespace Quizora.Application.Services;

public class TestService : ITestService
{
    private readonly ITestRepository _testRepository;
    private readonly IUserRepository _userRepository;

    public TestService(ITestRepository testRepository, IUserRepository userRepository)
    {
        _testRepository = testRepository;
        _userRepository = userRepository;
    }

    public async Task<Result<List<TestDto>>> GetMyTestsAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user?.Company == null)
            return Result<List<TestDto>>.Failure("Company not found");

        var tests = await _testRepository.GetByCompanyIdAsync(user.Company.Id);

        var result = tests.Select(t => new TestDto
        {
            Id = t.Id,
            Title = t.Title,
            Description = t.Description,
            Status = t.Status,
            DurationInMinutes = t.DurationInMinutes,
            PassingScore = t.PassingScore,
            PassingPercent = t.PassingPercent,
            TotalQuestions = t.Questions.Count,
            TotalInvitations = t.Invitations.Count,
            CompletedCount = t.Invitations.Count(i => i.Status == InvitationStatus.Completed),
            CreatedAt = t.CreatedAt
        }).ToList();

        return Result<List<TestDto>>.Success(result);
    }

    public async Task<Result<TestDto>> CreateTestAsync(Guid userId, CreateTestDto dto)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user?.Company == null)
            return Result<TestDto>.Failure("Company not found");

        var test = new Test
        {
            CompanyId = user.Company.Id,
            Title = dto.Title.Trim(),
            Description = dto.Description?.Trim(),
            DurationInMinutes = dto.DurationInMinutes,
            PassingScore = dto.PassingScore,
            PassingPercent = dto.PassingPercent,
            Status = TestStatus.Draft
        };

        await _testRepository.AddAsync(test);
        await _testRepository.SaveChangesAsync();

        var response = new TestDto
        {
            Id = test.Id,
            Title = test.Title,
            Description = test.Description,
            Status = test.Status,
            DurationInMinutes = test.DurationInMinutes,
            PassingScore = test.PassingScore,
            PassingPercent = test.PassingPercent,
            TotalQuestions = 0,
            TotalInvitations = 0,
            CompletedCount = 0,
            CreatedAt = test.CreatedAt
        };

        return Result<TestDto>.Success(response, "Test created successfully");
    }

    public async Task<Result> AddQuestionAsync(Guid userId, Guid testId, CreateQuestionDto dto)
    {
        var test = await _testRepository.GetByIdAsync(testId);
        if (test == null)
            return Result.Failure("Test not found");

        var user = await _userRepository.GetByIdAsync(userId);
        if (user?.Company?.Id != test.CompanyId)
            return Result.Failure("Unauthorized");

        var type = (dto.QuestionType ?? "MCQ").Trim();
        if (string.IsNullOrWhiteSpace(dto.Text))
            return Result.Failure("Question text is required");

        var question = new Question
        {
            TestId = testId,
            Text = dto.Text.Trim(),
            QuestionType = type,
            SampleInput = dto.SampleInput,
            SampleOutput = dto.SampleOutput,
            StarterCode = dto.StarterCode,
            Order = (test.Questions?.Count ?? 0) + 1,
            Options = new List<Option>()
        };

        if (type.Equals("MCQ", StringComparison.OrdinalIgnoreCase))
        {
            if (dto.Options == null || dto.Options.Count < 2)
                return Result.Failure("MCQ needs at least 2 options");
            if (dto.Options.Count(o => o.IsCorrect) != 1)
                return Result.Failure("Exactly one correct option required");
            question.Options = dto.Options.Select(o => new Option
            {
                Text = o.Text.Trim(),
                IsCorrect = o.IsCorrect
            }).ToList();
        }

        await _testRepository.AddQuestionAsync(question);
        return Result.Success("Question added successfully");
    }

    public async Task<Result> UpdateStatusAsync(Guid userId, Guid testId, TestStatus status)
    {
        var test = await _testRepository.GetByIdAsync(testId);
        if (test == null)
            return Result.Failure("Test not found");

        var user = await _userRepository.GetByIdAsync(userId);
        if (user?.Company?.Id != test.CompanyId)
            return Result.Failure("Unauthorized");

        test.Status = status;
        test.UpdatedAt = DateTime.UtcNow;
        await _testRepository.SaveChangesAsync();

        return Result.Success("Status updated");
    }
}