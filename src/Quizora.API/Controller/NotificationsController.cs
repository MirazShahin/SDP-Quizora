using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quizora.Application.Common;
using Quizora.Application.Interfaces;
using System.Security.Claims;

namespace Quizora.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Candidate")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationRepository _repo;

    public NotificationsController(INotificationRepository repo) => _repo = repo;

    [HttpGet]
    public async Task<IActionResult> GetMy()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var list = await _repo.GetByUserIdAsync(userId);
        var data = list.Select(n => new
        {
            n.Id,
            n.Title,
            n.Message,
            n.Type,
            n.IsRead,
            n.CreatedAt
        });
        return Ok(Result<object>.Success(data));
    }

    [HttpPost("{id}/read")]
    public async Task<IActionResult> MarkRead(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _repo.MarkAsReadAsync(id, userId);
        await _repo.SaveChangesAsync();
        return Ok(Result.Success());
    }

    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllRead()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _repo.MarkAllReadAsync(userId);
        await _repo.SaveChangesAsync();
        return Ok(Result.Success());
    }
}