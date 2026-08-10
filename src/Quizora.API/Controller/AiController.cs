using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quizora.Application.Common;
using Quizora.Application.Interfaces;

namespace Quizora.API.Controllers;

[ApiController]
[Route("api/ai")]
[Authorize(Roles = "Candidate")]
public class AiController : ControllerBase
{
    private readonly IAiService _ai;

    public AiController(IAiService ai)
    {
        _ai = ai;
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
    public async Task<ActionResult<Result<List<string>>>> WeakTopics()
    {
        try
        {
            // আপাতত ডামি হিস্ট্রি — পরে Practice attempt থেকে আনবেন
            var history = new List<TopicScoreDto>
            {
                new() { Topic = "OOP", Score = 3, Total = 10 },
                new() { Topic = "Database", Score = 7, Total = 10 },
                new() { Topic = "OS", Score = 2, Total = 10 },
                new() { Topic = "Networking", Score = 5, Total = 10 }
            };

            var topics = await _ai.GetWeakTopicsAsync(history);
            return Result<List<string>>.Success(topics);
        }
        catch (Exception ex)
        {
            return Result<List<string>>.Failure(ex.Message);
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