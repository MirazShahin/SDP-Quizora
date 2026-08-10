using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quizora.Application.Common;
using Quizora.Application.DTOs.Trivia;
using Quizora.Application.Interfaces;
using Quizora.Domain.Entities;
using Quizora.Infrastructure.Services;
using System.Security.Claims;

namespace Quizora.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PracticeController : ControllerBase
{
    private readonly IPracticeRepository _practiceRepository;
    private readonly QuizApiService _quizApi;

    public PracticeController(
        IPracticeRepository practiceRepository,
        QuizApiService quizApi)
    {
        _practiceRepository = practiceRepository;
        _quizApi = quizApi;
    }

    // ───────── DB categories ─────────

    [HttpGet("categories")]
    public async Task<ActionResult<Result<object>>> GetCategories()
    {
        var categories = await _practiceRepository.GetAllCategoriesAsync();
        var result = categories.Select(c => new
        {
            c.Id,
            c.Name,
            c.Description,
            c.Icon,
            c.Order
        });
        return Result<object>.Success(result);
    }

    [HttpGet("categories/{categoryId}/questions")]
    public async Task<ActionResult<Result<object>>> GetQuestions(Guid categoryId)
    {
        var category = await _practiceRepository.GetCategoryWithQuestionsAsync(categoryId);
        if (category == null)
            return Result<object>.Failure("Category not found");

        var questions = category.Questions.Select(q => new
        {
            q.Id,
            q.Text,
            q.Difficulty,
            q.Order,
            Options = q.Options.Select(o => new
            {
                o.Id,
                o.Text
            })
        });
        return Result<object>.Success(questions);
    }

    [HttpPost("submit")]
    [Authorize]
    public async Task<ActionResult<Result<object>>> Submit([FromBody] PracticeSubmitDto dto)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var category = await _practiceRepository.GetCategoryWithQuestionsAsync(dto.CategoryId);
        if (category == null)
            return Result<object>.Failure("Category not found");

        int score = 0;
        var total = category.Questions.Count;

        foreach (var answer in dto.Answers)
        {
            var question = category.Questions.FirstOrDefault(q => q.Id == answer.QuestionId);
            if (question == null) continue;

            var correctOption = question.Options.FirstOrDefault(o => o.IsCorrect);
            if (correctOption != null && correctOption.Id == answer.SelectedOptionId)
                score++;
        }

        var attempt = new PracticeAttempt
        {
            UserId = userId,
            CategoryId = dto.CategoryId,
            Score = score,
            TotalQuestions = total,
            CompletedAt = DateTime.UtcNow
        };

        await _practiceRepository.AddAttemptAsync(attempt);
        await _practiceRepository.SaveChangesAsync();

        return Result<object>.Success(new
        {
            Score = score,
            TotalQuestions = total,
            Percentage = total == 0 ? 0 : Math.Round((double)score / total * 100, 2)
        }, "Practice submitted successfully");
    }

    [HttpGet("my-attempts")]
    [Authorize]
    public async Task<ActionResult<Result<object>>> GetMyAttempts()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var attempts = await _practiceRepository.GetAttemptsByUserAsync(userId);

        var result = attempts.Select(a => new
        {
            a.Id,
            CategoryName = a.Category.Name,
            a.Score,
            a.TotalQuestions,
            Percentage = a.TotalQuestions == 0
                ? 0
                : Math.Round((double)a.Score / a.TotalQuestions * 100, 2),
            a.CompletedAt
        });
        return Result<object>.Success(result);
    }

    // ───────── QuizAPI.io (IT) ─────────

    /// <summary>
    /// exclude = pipe-separated question fingerprints (to avoid repeats)
    /// </summary>
    [HttpGet("quizapi")]
    [AllowAnonymous]
    public async Task<IActionResult> GetFromQuizApi(
        [FromQuery] int limit = 10,
        [FromQuery] string category = "Linux",
        [FromQuery] string difficulty = "Easy",
        [FromQuery] string? exclude = null)
    {
        try
        {
            if (limit < 1 || limit > 20)
                return Ok(Result<object>.Failure("limit must be 1-20"));

            List<string>? excludeList = null;
            if (!string.IsNullOrWhiteSpace(exclude))
            {
                excludeList = exclude
                    .Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(s =>
                    {
                        try { return Uri.UnescapeDataString(s); }
                        catch { return s; }
                    })
                    .Where(s => s.Length > 0)
                    .Take(100)
                    .ToList();
            }

            var quiz = await _quizApi.StartQuizAsync(limit, category, difficulty, excludeList);

            if (quiz.Questions.Count == 0)
                return Ok(Result<object>.Failure(
                    "No new questions left. Try another category or clear mock history."));

            return Ok(Result<object>.Success(quiz));
        }
        catch (Exception ex)
        {
            var msg = ex.InnerException?.Message ?? ex.Message;
            return Ok(Result<object>.Failure($"QuizAPI error: {msg}"));
        }
    }

    [HttpPost("quizapi/check-answers")]
    [AllowAnonymous]
    public IActionResult CheckQuizApiAnswers([FromBody] TriviaSubmitDto dto)
    {
        try
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.QuizId))
                return Ok(Result<object>.Failure("QuizId is required"));

            var score = _quizApi.ScoreQuiz(dto);
            if (score == null)
                return Ok(Result<object>.Failure("Quiz expired or not found. Please start again."));

            return Ok(Result<object>.Success(score));
        }
        catch (Exception ex)
        {
            var msg = ex.InnerException?.Message ?? ex.Message;
            return Ok(Result<object>.Failure($"QuizAPI score error: {msg}"));
        }
    }
}

public class PracticeSubmitDto
{
    public Guid CategoryId { get; set; }
    public List<PracticeAnswerDto> Answers { get; set; } = new();
}

public class PracticeAnswerDto
{
    public Guid QuestionId { get; set; }
    public Guid SelectedOptionId { get; set; }
}