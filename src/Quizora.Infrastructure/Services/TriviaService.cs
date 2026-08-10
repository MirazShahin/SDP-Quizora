using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Quizora.Application.DTOs.Trivia;

namespace Quizora.Infrastructure.Services;

public class TriviaService
{
    private readonly HttpClient _http;
    private readonly IMemoryCache _cache;
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };
    private static readonly TimeSpan QuizTtl = TimeSpan.FromMinutes(45);

    public TriviaService(HttpClient http, IMemoryCache cache)
    {
        _http = http;
        _cache = cache;
        if (_http.BaseAddress == null)
            _http.BaseAddress = new Uri("https://opentdb.com/");
    }

    public async Task<List<PracticeQuestionFromApi>> GetQuestionsAsync(
        int amount = 10, string difficulty = "medium", int category = 18)
    {
        amount = Math.Clamp(amount, 1, 50);
        difficulty = (difficulty ?? "medium").Trim().ToLowerInvariant();
        if (difficulty is not ("easy" or "medium" or "hard")) difficulty = "medium";

        var url = $"api.php?amount={amount}&category={category}&difficulty={difficulty}&type=multiple";
        TriviaApiResponse? response = null;
        try { response = await _http.GetFromJsonAsync<TriviaApiResponse>(url, JsonOpts); }
        catch { /* ignore */ }

        if (response == null || response.ResponseCode != 0 || response.Results == null || response.Results.Count == 0)
        {
            try
            {
                url = $"api.php?amount={amount}&difficulty={difficulty}&type=multiple";
                response = await _http.GetFromJsonAsync<TriviaApiResponse>(url, JsonOpts);
            }
            catch { return new(); }
        }

        if (response == null || response.ResponseCode != 0 || response.Results == null)
            return new();

        var questions = new List<PracticeQuestionFromApi>();
        foreach (var q in response.Results)
        {
            var options = q.IncorrectAnswers
                .Select(a => new PracticeOptionFromApi { Text = DecodeHtml(a), IsCorrect = false })
                .ToList();

            options.Add(new PracticeOptionFromApi { Text = DecodeHtml(q.CorrectAnswer), IsCorrect = true });
            options = options.OrderBy(_ => Guid.NewGuid()).ToList();

            questions.Add(new PracticeQuestionFromApi
            {
                Text = DecodeHtml(q.Question),
                Difficulty = q.Difficulty,
                Category = DecodeHtml(q.Category),
                Options = options
            });
        }
        return questions;
    }

    public async Task<TriviaQuizStartDto> StartQuizAsync(
        int amount = 10, string difficulty = "medium", int category = 18)
    {
        var full = await GetQuestionsAsync(amount, difficulty, category);
        var quizId = Guid.NewGuid().ToString("N");
        _cache.Set($"trivia-quiz:{quizId}", full, QuizTtl);

        return new TriviaQuizStartDto
        {
            QuizId = quizId,
            Questions = full.Select(q => new TriviaQuizQuestionDto
            {
                Id = q.Id,
                Text = q.Text,
                Difficulty = q.Difficulty,
                Category = q.Category,
                Options = q.Options.Select(o => new TriviaQuizOptionDto
                {
                    Id = o.Id,
                    Text = o.Text
                }).ToList()
            }).ToList()
        };
    }

    public TriviaScoreDto? ScoreQuiz(TriviaSubmitDto submit)
    {
        if (string.IsNullOrWhiteSpace(submit.QuizId)) return null;

        if (!_cache.TryGetValue($"trivia-quiz:{submit.QuizId}", out List<PracticeQuestionFromApi>? stored)
            || stored == null)
            return null;

        int score = 0;
        var review = new List<TriviaReviewItemDto>();

        foreach (var q in stored)
        {
            var ans = submit.Answers.FirstOrDefault(a => a.QuestionId == q.Id);
            var selected = q.Options.FirstOrDefault(o => o.Id == ans?.SelectedOptionId);
            var correct = q.Options.FirstOrDefault(o => o.IsCorrect);
            var isCorrect = selected != null && selected.IsCorrect;
            if (isCorrect) score++;

            review.Add(new TriviaReviewItemDto
            {
                QuestionId = q.Id,
                QuestionText = q.Text,
                SelectedOptionText = selected?.Text ?? "—",
                CorrectOptionText = correct?.Text ?? "—",
                IsCorrect = isCorrect
            });
        }

        _cache.Remove($"trivia-quiz:{submit.QuizId}");

        return new TriviaScoreDto
        {
            Score = score,
            TotalQuestions = stored.Count,
            Percentage = stored.Count == 0 ? 0 : Math.Round(score * 100.0 / stored.Count, 2),
            Review = review
        };
    }

    private static string DecodeHtml(string text) =>
        string.IsNullOrEmpty(text) ? text : WebUtility.HtmlDecode(text);
}