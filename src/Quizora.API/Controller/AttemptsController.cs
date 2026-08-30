using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quizora.Application.Common;
using Quizora.Application.DTOs.Attempts;
using Quizora.Application.DTOs.Questions;
using Quizora.Application.Interfaces;
using System.Security.Claims;

namespace Quizora.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Candidate")]
public class AttemptsController : ControllerBase
{
    private readonly IAttemptService _attemptService;

    public AttemptsController(IAttemptService attemptService)
    {
        _attemptService = attemptService;
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
     
    [HttpGet("{invitationId}/questions")]
    public async Task<ActionResult<Result<List<QuestionDto>>>> GetQuestions(Guid invitationId)
    {
        var result = await _attemptService.GetQuestionsAsync(GetUserId(), invitationId);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }
     
    [HttpPost("{invitationId}/submit")]
    public async Task<ActionResult<Result<ResultDto>>> Submit(Guid invitationId, SubmitTestDto dto)
    {
        var result = await _attemptService.SubmitTestAsync(GetUserId(), invitationId, dto);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }
}