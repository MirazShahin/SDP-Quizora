using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quizora.Application.Common;
using Quizora.Application.DTOs.Invitations;
using Quizora.Application.Interfaces;
using System.Security.Claims;

namespace Quizora.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InvitationsController : ControllerBase
{
    private readonly IInvitationService _invitationService;

    public InvitationsController(IInvitationService invitationService)
    {
        _invitationService = invitationService;
    }


    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
     
    [HttpGet("my")]
    [Authorize(Roles = "Candidate")]
    public async Task<ActionResult<Result<List<InvitationDto>>>> GetMyInvitations()
    {
        var result = await _invitationService.GetMyInvitationsAsync(GetUserId());

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }
     
    [HttpPost]
    [Authorize(Roles = "Company")]
    public async Task<ActionResult<Result>> Invite(InviteCandidateDto dto)
    {
        var result = await _invitationService.InviteCandidateAsync(GetUserId(), dto);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("bulk")]
    [Authorize(Roles = "Company")]
    public async Task<IActionResult> BulkInvite([FromBody] BulkInviteDto dto)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _invitationService.BulkInviteAsync(userId, dto);
        return Ok(result);
    }
}