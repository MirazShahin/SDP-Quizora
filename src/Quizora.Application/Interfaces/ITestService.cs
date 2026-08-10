using Quizora.Application.Common;
using Quizora.Application.DTOs.Questions;
using Quizora.Application.DTOs.Tests;
using Quizora.Domain.Enums;

namespace Quizora.Application.Interfaces;

public interface ITestService
{
    Task<Result<List<TestDto>>> GetMyTestsAsync(Guid userId);
    Task<Result<TestDto>> CreateTestAsync(Guid userId, CreateTestDto dto);
    Task<Result> AddQuestionAsync(Guid userId, Guid testId, CreateQuestionDto dto);
    Task<Result> UpdateStatusAsync(Guid userId, Guid testId, TestStatus status);
}