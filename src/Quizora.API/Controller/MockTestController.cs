using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quizora.Application.Common;
using Quizora.Application.Interfaces;
using Quizora.Domain.Entities;
using System.Security.Claims;

namespace Quizora.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MockTestController : ControllerBase
{
    private readonly IMockTestRepository _mockRepo;

    public MockTestController(IMockTestRepository mockRepo)
    {
        _mockRepo = mockRepo;
    }

    // সব অ্যাকটিভ মক টেস্ট
    [HttpGet]
    public async Task<ActionResult<Result<object>>> GetAll()
    {
        var tests = await _mockRepo.GetAllActiveAsync();

        var result = tests.Select(t => new
        {
            t.Id,
            t.Title,
            t.Description,
            t.DurationInMinutes,
            t.TotalQuestions
        });

        return Result<object>.Success(result);
    }

    // মক টেস্টের প্রশ্নগুলো (সঠিক উত্তর ছাড়া)
    [HttpGet("{id}/questions")]
    [Authorize]
    public async Task<ActionResult<Result<object>>> GetQuestions(Guid id)
    {
        var test = await _mockRepo.GetByIdWithQuestionsAsync(id);
        if (test == null)
            return Result<object>.Failure("Mock test not found");

        var questions = test.Questions.Select(mq => new
        {
            QuestionId = mq.PracticeQuestion.Id,
            mq.PracticeQuestion.Text,
            mq.PracticeQuestion.Difficulty,
            mq.Order,
            Options = mq.PracticeQuestion.Options.Select(o => new
            {
                o.Id,
                o.Text
            })
        });

        return Result<object>.Success(new
        {
            test.Title,
            test.DurationInMinutes,
            Questions = questions
        });
    }

    // মক টেস্ট সাবমিট
    [HttpPost("submit")]
    [Authorize]
    public async Task<ActionResult<Result<object>>> Submit([FromBody] MockSubmitDto dto)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var test = await _mockRepo.GetByIdWithQuestionsAsync(dto.MockTestId);
        if (test == null)
            return Result<object>.Failure("Mock test not found");

        int score = 0;
        var total = test.Questions.Count;

        foreach (var answer in dto.Answers)
        {
            var mq = test.Questions.FirstOrDefault(q => q.PracticeQuestionId == answer.QuestionId);
            if (mq == null) continue;

            var correct = mq.PracticeQuestion.Options.FirstOrDefault(o => o.IsCorrect);
            if (correct != null && correct.Id == answer.SelectedOptionId)
                score++;
        }

        var attempt = new MockAttempt
        {
            UserId = userId,
            MockTestId = dto.MockTestId,
            Score = score,
            TotalQuestions = total,
            TimeTakenInSeconds = dto.TimeTakenInSeconds,
            StartedAt = dto.StartedAt,
            CompletedAt = DateTime.UtcNow
        };

        await _mockRepo.AddAttemptAsync(attempt);
        await _mockRepo.SaveChangesAsync();

        return Result<object>.Success(new
        {
            Score = score,
            TotalQuestions = total,
            Percentage = total == 0 ? 0 : Math.Round((double)score / total * 100, 2),
            TimeTakenInSeconds = dto.TimeTakenInSeconds
        }, "Mock test submitted");
    }

    // লিডারবোর্ড
    [HttpGet("{id}/leaderboard")]
    public async Task<ActionResult<Result<object>>> GetLeaderboard(Guid id)
    {
        var attempts = await _mockRepo.GetLeaderboardAsync(id);

        var result = attempts.Select((a, index) => new
        {
            Rank = index + 1,
            FullName = a.User.FullName,
            a.Score,
            a.TotalQuestions,
            Percentage = a.TotalQuestions == 0 ? 0 : Math.Round((double)a.Score / a.TotalQuestions * 100, 2),
            a.TimeTakenInSeconds,
            a.CompletedAt
        });

        return Result<object>.Success(result);
    }

    // নিজের মক টেস্ট হিস্ট্রি
    [HttpGet("my-attempts")]
    [Authorize]
    public async Task<ActionResult<Result<object>>> GetMyAttempts()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var attempts = await _mockRepo.GetAttemptsByUserAsync(userId);

        var result = attempts.Select(a => new
        {
            a.Id,
            TestTitle = a.MockTest.Title,
            a.Score,
            a.TotalQuestions,
            a.TimeTakenInSeconds,
            a.CompletedAt
        });

        return Result<object>.Success(result);
    }
}

public class MockSubmitDto
{
    public Guid MockTestId { get; set; }
    public int TimeTakenInSeconds { get; set; }
    public DateTime StartedAt { get; set; }
    public List<MockAnswerDto> Answers { get; set; } = new();
}

public class MockAnswerDto
{
    public Guid QuestionId { get; set; }
    public Guid SelectedOptionId { get; set; }
}