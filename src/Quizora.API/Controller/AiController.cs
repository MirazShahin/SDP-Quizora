using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Quizora.Application.Common;
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
        if (string.IsNullOrWhiteSpace(req.Question))
            return Result<string>.Failure("Question is required");

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
        if (string.IsNullOrWhiteSpace(req.Message))
            return Result<string>.Failure("Message is required");

        try
        {
            var history = (req.History ?? new())
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

    [HttpGet("weak-topics")]
    public async Task<IActionResult> WeakTopics()
    {
        try
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
                return Ok(Result<List<WeakTopicDto>>.Failure("Not authenticated"));

            var userId = Guid.Parse(userIdClaim);

            // ── 1) Practice attempts by category (main signal) ──
            var practice = await _db.PracticeAttempts
                .Include(a => a.Category)
                .Where(a => a.UserId == userId)
                .ToListAsync();

            var fromPractice = practice
                .GroupBy(a => a.Category?.Name ?? "Unknown")
                .Select(g => new
                {
                    Topic = g.Key,
                    Score = g.Sum(x => x.Score),
                    Total = g.Sum(x => x.TotalQuestions),
                    Source = "Practice"
                })
                .Where(x => x.Total > 0)
                .ToList();

            // ── 2) Mock attempts by test title (extra signal) ──
            var mocks = await _db.MockAttempts
                .Include(a => a.MockTest)
                .Where(a => a.UserId == userId)
                .ToListAsync();

            var fromMock = mocks
                .GroupBy(a => a.MockTest?.Title ?? "Mock")
                .Select(g => new
                {
                    Topic = g.Key,
                    Score = g.Sum(x => x.Score),
                    Total = g.Sum(x => x.TotalQuestions),
                    Source = "Mock"
                })
                .Where(x => x.Total > 0)
                .ToList();

            var combined = fromPractice.Concat(fromMock).ToList();

            if (combined.Count == 0)
            {
                return Ok(Result<List<WeakTopicDto>>.Success(new List<WeakTopicDto>(),
                    "No practice history yet. Complete some practice or mock tests."));
            }

            // ── 3) Own formula: weak if accuracy < 60% and at least 3 questions ──
            var weak = combined
                .Select(x =>
                {
                    var acc = x.Total == 0 ? 0 : Math.Round(100.0 * x.Score / x.Total, 1);
                    var severity = acc < 40 ? "High" : acc < 60 ? "Medium" : "Low";
                    return new WeakTopicDto
                    {
                        Topic = x.Topic,
                        Score = x.Score,
                        Total = x.Total,
                        Accuracy = acc,
                        Severity = severity,
                        Source = x.Source
                    };
                })
                .Where(x => x.Total >= 3 && x.Accuracy < 60) // নিজের business rule
                .OrderBy(x => x.Accuracy)
                .ThenByDescending(x => x.Total)
                .Take(8)
                .ToList();

            // সব ৬০%+ হলে তবুও lowest 3 দেখাও (encouragement)
            if (weak.Count == 0)
            {
                weak = combined
                    .Select(x =>
                    {
                        var acc = x.Total == 0 ? 0 : Math.Round(100.0 * x.Score / x.Total, 1);
                        return new WeakTopicDto
                        {
                            Topic = x.Topic,
                            Score = x.Score,
                            Total = x.Total,
                            Accuracy = acc,
                            Severity = "Low",
                            Source = x.Source
                        };
                    })
                    .OrderBy(x => x.Accuracy)
                    .Take(3)
                    .ToList();
            }

            return Ok(Result<List<WeakTopicDto>>.Success(weak));
        }
        catch (Exception ex)
        {
            return Ok(Result<List<WeakTopicDto>>.Failure(ex.Message));
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