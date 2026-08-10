using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Quizora.Application.DTOs.Trivia;

namespace Quizora.Infrastructure.Services;

public class QuizApiService
{
    private readonly HttpClient _http;
    private readonly IMemoryCache _cache;
    private readonly string _apiKey;
    private static readonly TimeSpan QuizTtl = TimeSpan.FromMinutes(45);
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public QuizApiService(HttpClient http, IMemoryCache cache, IConfiguration config)
    {
        _http = http;
        _cache = cache;
        _apiKey = (config["QuizApi:ApiKey"] ?? "").Trim();

        if (_http.BaseAddress == null)
            _http.BaseAddress = new Uri(config["QuizApi:BaseUrl"] ?? "https://quizapi.io/api/v1/");

        _http.DefaultRequestHeaders.Remove("Authorization");
        _http.DefaultRequestHeaders.Remove("X-Api-Key");
        if (!string.IsNullOrWhiteSpace(_apiKey))
            _http.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Bearer {_apiKey}");
    }

    public async Task<List<PracticeQuestionFromApi>> GetQuestionsAsync(
        int limit = 10,
        string category = "Linux",
        string difficulty = "Easy")
    {
        limit = Math.Clamp(limit, 1, 20);

        if (string.IsNullOrWhiteSpace(_apiKey))
            throw new InvalidOperationException(
                "QuizApi:ApiKey is missing in appsettings. Add your key from https://quizapi.io");

        category = category?.Trim() ?? "";
        var diff = string.IsNullOrWhiteSpace(difficulty) ? "" : NormalizeDifficulty(difficulty);

        var url = $"questions?limit={limit}&api_key={Uri.EscapeDataString(_apiKey)}";
        if (!string.IsNullOrWhiteSpace(category))
            url += $"&category={Uri.EscapeDataString(category)}";
        if (!string.IsNullOrWhiteSpace(diff))
            url += $"&difficulty={Uri.EscapeDataString(diff)}";

        using var response = await _http.GetAsync(url);
        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"QuizAPI HTTP {(int)response.StatusCode}: {Trim(json, 400)}");

        if (string.IsNullOrWhiteSpace(json))
            return new();

        var trimmed = json.TrimStart();
        var list = new List<PracticeQuestionFromApi>();

        // New format: { success, data: [...] }
        if (trimmed.StartsWith("{"))
        {
            var envelope = JsonSerializer.Deserialize<QuizApiEnvelope>(json, JsonOpts);
            if (envelope?.Data != null)
            {
                foreach (var q in envelope.Data)
                {
                    if (string.IsNullOrWhiteSpace(q.Text) || q.Answers == null || q.Answers.Count < 2)
                        continue;

                    list.Add(new PracticeQuestionFromApi
                    {
                        Id = string.IsNullOrWhiteSpace(q.Id) ? Guid.NewGuid().ToString() : q.Id,
                        Text = q.Text.Trim(),
                        Difficulty = string.IsNullOrWhiteSpace(q.Difficulty) ? (diff.Length > 0 ? diff : "MEDIUM") : q.Difficulty,
                        Category = string.IsNullOrWhiteSpace(q.Category) ? category : q.Category,
                        Options = q.Answers.Select(a => new PracticeOptionFromApi
                        {
                            Id = string.IsNullOrWhiteSpace(a.Id) ? Guid.NewGuid().ToString() : a.Id,
                            Text = a.Text,
                            IsCorrect = a.IsCorrect
                        }).OrderBy(_ => Guid.NewGuid()).ToList()
                    });
                }
            }

            if (list.Count == 0 && !string.IsNullOrWhiteSpace(diff))
                return await GetQuestionsWithoutDifficulty(limit, category);

            return list;
        }

        // Classic format: [ ... ]
        if (trimmed.StartsWith("["))
        {
            var raw = JsonSerializer.Deserialize<List<QuizApiQuestion>>(json, JsonOpts) ?? new();
            foreach (var q in raw)
            {
                if (string.IsNullOrWhiteSpace(q.Question)) continue;
                var options = BuildOptions(q);
                if (options.Count < 2) continue;

                list.Add(new PracticeQuestionFromApi
                {
                    Text = q.Question.Trim(),
                    Difficulty = q.Difficulty ?? diff,
                    Category = q.Category ?? category,
                    Options = options.OrderBy(_ => Guid.NewGuid()).ToList()
                });
            }
            return list;
        }

        throw new InvalidOperationException($"QuizAPI unexpected response: {Trim(json, 300)}");
    }

    private async Task<List<PracticeQuestionFromApi>> GetQuestionsWithoutDifficulty(int limit, string category)
    {
        var url = $"questions?limit={limit}&api_key={Uri.EscapeDataString(_apiKey)}";
        if (!string.IsNullOrWhiteSpace(category))
            url += $"&category={Uri.EscapeDataString(category)}";

        using var response = await _http.GetAsync(url);
        var json = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode) return new();

        var envelope = JsonSerializer.Deserialize<QuizApiEnvelope>(json, JsonOpts);

        if (envelope?.Data == null || envelope.Data.Count == 0)
        {
            url = $"questions?limit={limit}&api_key={Uri.EscapeDataString(_apiKey)}";
            using var r2 = await _http.GetAsync(url);
            json = await r2.Content.ReadAsStringAsync();
            if (!r2.IsSuccessStatusCode) return new();
            envelope = JsonSerializer.Deserialize<QuizApiEnvelope>(json, JsonOpts);
        }

        if (envelope?.Data == null) return new();

        return envelope.Data
            .Where(q => !string.IsNullOrWhiteSpace(q.Text) && q.Answers != null && q.Answers.Count >= 2)
            .Select(q => new PracticeQuestionFromApi
            {
                Id = string.IsNullOrWhiteSpace(q.Id) ? Guid.NewGuid().ToString() : q.Id,
                Text = q.Text.Trim(),
                Difficulty = q.Difficulty ?? "MEDIUM",
                Category = q.Category ?? category,
                Options = q.Answers.Select(a => new PracticeOptionFromApi
                {
                    Id = string.IsNullOrWhiteSpace(a.Id) ? Guid.NewGuid().ToString() : a.Id,
                    Text = a.Text,
                    IsCorrect = a.IsCorrect
                }).OrderBy(_ => Guid.NewGuid()).ToList()
            })
            .ToList();
    }

    public async Task<TriviaQuizStartDto> StartQuizAsync(
        int limit = 10,
        string category = "Linux",
        string difficulty = "Easy",
        List<string>? excludeFingerprints = null)
    {
        limit = Math.Clamp(limit, 1, 20);
        excludeFingerprints ??= new();

        var exclude = new HashSet<string>(
            excludeFingerprints.Select(Fingerprint).Where(x => x.Length > 0),
            StringComparer.OrdinalIgnoreCase);

        var pool = new List<PracticeQuestionFromApi>();
        var attempts = 0;

        while (pool.Count < limit && attempts < 5)
        {
            attempts++;
            var batchSize = Math.Min(20, Math.Max(limit, 10));

            List<PracticeQuestionFromApi> batch;
            if (attempts <= 2)
                batch = await GetQuestionsAsync(batchSize, category, difficulty);
            else if (attempts == 3)
                batch = await GetQuestionsAsync(batchSize, category, "");
            else
                batch = await GetQuestionsAsync(batchSize, "", "");

            foreach (var q in batch)
            {
                var fp = Fingerprint(q.Text);
                if (fp.Length == 0) continue;
                if (exclude.Contains(fp)) continue;
                if (pool.Any(p => Fingerprint(p.Text) == fp)) continue;

                pool.Add(q);
                exclude.Add(fp);
                if (pool.Count >= limit) break;
            }
        }

        var selected = pool.Take(limit).ToList();
        var quizId = Guid.NewGuid().ToString("N");

        if (selected.Count > 0)
            _cache.Set($"quizapi-quiz:{quizId}", selected, QuizTtl);

        return new TriviaQuizStartDto
        {
            QuizId = quizId,
            Questions = selected.Select(q => new TriviaQuizQuestionDto
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
        if (submit == null || string.IsNullOrWhiteSpace(submit.QuizId))
            return null;

        if (!_cache.TryGetValue($"quizapi-quiz:{submit.QuizId}", out List<PracticeQuestionFromApi>? stored)
            || stored == null)
            return null;

        int score = 0;
        var review = new List<TriviaReviewItemDto>();
        var answers = submit.Answers ?? new();

        foreach (var q in stored)
        {
            var ans = answers.FirstOrDefault(a => a.QuestionId == q.Id);
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

        _cache.Remove($"quizapi-quiz:{submit.QuizId}");

        return new TriviaScoreDto
        {
            Score = score,
            TotalQuestions = stored.Count,
            Percentage = stored.Count == 0 ? 0 : Math.Round(score * 100.0 / stored.Count, 2),
            Review = review
        };
    }

    private static List<PracticeOptionFromApi> BuildOptions(QuizApiQuestion q)
    {
        var answers = q.Answers ?? new QuizApiAnswers();
        var correct = q.CorrectAnswers ?? new QuizApiCorrectAnswers();

        var pairs = new (string? text, string? isCorrect)[]
        {
            (answers.AnswerA, correct.AnswerACorrect),
            (answers.AnswerB, correct.AnswerBCorrect),
            (answers.AnswerC, correct.AnswerCCorrect),
            (answers.AnswerD, correct.AnswerDCorrect),
            (answers.AnswerE, correct.AnswerECorrect),
            (answers.AnswerF, correct.AnswerFCorrect),
        };

        return pairs
            .Where(p => !string.IsNullOrWhiteSpace(p.text))
            .Select(p => new PracticeOptionFromApi
            {
                Text = p.text!.Trim(),
                IsCorrect = string.Equals(p.isCorrect, "true", StringComparison.OrdinalIgnoreCase)
            })
            .ToList();
    }

    private static string Fingerprint(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return "";
        var chars = text.Trim().ToLowerInvariant().ToCharArray();
        var sb = new System.Text.StringBuilder(chars.Length);
        bool space = false;
        foreach (var ch in chars)
        {
            if (char.IsWhiteSpace(ch))
            {
                if (!space) { sb.Append(' '); space = true; }
            }
            else
            {
                sb.Append(ch);
                space = false;
            }
        }
        return sb.ToString().Trim();
    }

    private static string NormalizeDifficulty(string? d)
    {
        d = (d ?? "Easy").Trim();
        if (d.Equals("easy", StringComparison.OrdinalIgnoreCase)) return "EASY";
        if (d.Equals("medium", StringComparison.OrdinalIgnoreCase)) return "MEDIUM";
        if (d.Equals("hard", StringComparison.OrdinalIgnoreCase)) return "HARD";
        return d.ToUpperInvariant();
    }

    private static string Trim(string s, int max)
        => string.IsNullOrEmpty(s) || s.Length <= max ? s : s[..max] + "...";
}