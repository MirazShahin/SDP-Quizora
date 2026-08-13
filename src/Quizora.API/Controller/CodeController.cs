using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quizora.Application.Common;
using Quizora.Application.DTOs.Code;
using Quizora.Application.Interfaces;

namespace Quizora.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Candidate")]
public class CodeController : ControllerBase
{
    private readonly ICodeExecutionService _runner;

    public CodeController(ICodeExecutionService runner)
    {
        _runner = runner;
    }

    /// <summary>Run C or C++ — own gcc/g++ engine</summary>
    [HttpPost("run")]
    public async Task<IActionResult> Run([FromBody] CodeRunRequestDto dto, CancellationToken ct)
    {
        try
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.SourceCode))
                return Ok(Result<CodeRunResultDto>.Failure("Source code is required"));

            var result = await _runner.RunAsync(dto, ct);
            return Ok(Result<CodeRunResultDto>.Success(result));
        }
        catch (Exception ex)
        {
            return Ok(Result<CodeRunResultDto>.Failure(ex.Message));
        }
    }
}