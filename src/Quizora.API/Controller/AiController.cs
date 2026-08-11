using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Quizora.Application.Common;
using Quizora.Application.DTOs.Ai;
using Quizora.Application.DTOs.Questions;
using Quizora.Application.Interfaces;
using Quizora.Infrastructure.Persistence;
using System.Security.Claims;

namespace Quizora.API.Controllers;

[ApiController]
[Route("api/ai")]
[Authorize(Roles = "Candidate")]
public class AiController : ControllerBase
{
    private readonly IAiService _ai;
    private readonly ApplicationDbContext _db;

    public AiController(IAiService ai, ApplicationDbContext db)
    {
        _ai = ai;
        _db = db;
    }

    [HttpPost("coach")]
    public async Task<ActionResult<Result<string>>> Coach([FromBody] CoachRequest req)
    {
        try
        {
            var text = await _ai.GetCoachFeedbackAsync(req.Question, req.UserAnswer);
            return Result<string>.Success(text);
        }
        catch (Exception ex)
        {
            return Result<string>.Failure(ex.Message);
        }
    }

    [HttpPost("mock-interview")]
    public async Task<ActionResult<Result<string>>> MockInterview([FromBody] MockRequest req)
    {
        try
        {
            var history = (req.History ?? new List<ChatMsg>())
                .Select(h => new ChatMessageDto { Role = h.Role, Content = h.Content })
                .ToList();

            var text = await _ai.GetMockInterviewReplyAsync(history, req.Message);
            return Result<string>.Success(text);
        }
        catch (Exception ex)
        {
            return Result<string>.Failure(ex.Message);
        }
    }

    /// <summary>Own algorithm: practice/mock accuracy by category — not hardcoded AI list.</summary>
    [HttpGet("weak-topics")]
    public async Task<IActionResult> WeakTopics()
    {
        try
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
                return Ok(Result<PerformanceSummaryDto>.Failure("Not authenticated"));

            var userId = Guid.Parse(userIdClaim);

            var practice = await _db.PracticeAttempts
                .Include(a => a.Category)
                .Where(a => a.UserId == userId)
                .ToListAsync();

            var mocks = await _db.MockAttempts
                .Include(a => a.MockTest)
                .Where(a => a.UserId == userId)
                .ToListAsync();

            var rows = new List<WeakTopicDto>();

            foreach (var g in practice.GroupBy(a => a.Category?.Name ?? "Practice"))
            {
                var score = g.Sum(x => x.Score);
                var total = g.Sum(x => x.TotalQuestions);
                if (total <= 0) continue;
                var acc = Math.Round(100.0 * score / total, 1);
                rows.Add(new WeakTopicDto
                {
                    Topic = g.Key,
                    Score = score,
                    Total = total,
                    Accuracy = acc,
                    Severity = acc < 40 ? "High" : acc < 60 ? "Medium" : "Low",
                    Source = "Practice"
                });
            }

            foreach (var g in mocks.GroupBy(a => a.MockTest?.Title ?? "Mock"))
            {
                var score = g.Sum(x => x.Score);
                var total = g.Sum(x => x.TotalQuestions);
                if (total <= 0) continue;
                var acc = Math.Round(100.0 * score / total, 1);
                rows.Add(new WeakTopicDto
                {
                    Topic = g.Key,
                    Score = score,
                    Total = total,
                    Accuracy = acc,
                    Severity = acc < 40 ? "High" : acc < 60 ? "Medium" : "Low",
                    Source = "Mock"
                });
            }

            var weak = rows
                .Where(x => x.Total >= 3 && x.Accuracy < 60)
                .OrderBy(x => x.Accuracy)
                .Take(8)
                .ToList();

            if (weak.Count == 0 && rows.Count > 0)
                weak = rows.OrderBy(x => x.Accuracy).Take(3).ToList();

            var allTotal = rows.Sum(r => r.Total);
            var allScore = rows.Sum(r => r.Score);
            var avg = allTotal == 0 ? 0 : Math.Round(100.0 * allScore / allTotal, 1);
            var strongest = rows.OrderByDescending(r => r.Accuracy).FirstOrDefault()?.Topic;

            var summary = new PerformanceSummaryDto
            {
                PracticeSessions = practice.Count,
                MockSessions = mocks.Count,
                AvgAccuracy = avg,
                WeakCount = weak.Count(w => w.Accuracy < 60),
                StrongestTopic = strongest,
                WeakTopics = weak
            };

            return Ok(Result<PerformanceSummaryDto>.Success(summary));
        }
        catch (Exception ex)
        {
            return Ok(Result<PerformanceSummaryDto>.Failure(ex.Message));
        }
    }

    public class CoachRequest
    {
        public string Question { get; set; } = "";
        public string? UserAnswer { get; set; }
    }

    public class MockRequest
    {
        public string Message { get; set; } = "";
        public List<ChatMsg>? History { get; set; }
    }

    public class ChatMsg
    {
        public string Role { get; set; } = "user";
        public string Content { get; set; } = "";
    }
}
