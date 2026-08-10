using Quizora.Application.Common;
using Quizora.Application.DTOs.Attempts;
using Quizora.Application.DTOs.Questions;

namespace Quizora.Application.Interfaces;

public interface IAttemptService
{
    Task<Result<List<QuestionDto>>> GetQuestionsAsync(Guid userId, Guid invitationId);
    Task<Result<ResultDto>> SubmitTestAsync(Guid userId, Guid invitationId, SubmitTestDto dto);
    Task<Result<ResultDto>> GetResultAsync(Guid userId, Guid invitationId);
    Task<Result<List<ResultDto>>> GetResultsByTestAsync(Guid userId, Guid testId);
}