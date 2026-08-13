using Quizora.Application.DTOs.Code;

namespace Quizora.Application.Interfaces;

public interface ICodeExecutionService
{
    Task<CodeRunResultDto> RunAsync(CodeRunRequestDto request, CancellationToken ct = default);
}