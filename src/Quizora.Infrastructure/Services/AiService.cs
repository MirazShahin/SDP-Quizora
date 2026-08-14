using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Quizora.Application.Interfaces;

namespace Quizora.Infrastructure.Services;

public class AiService : IAiService
{
    private readonly HttpClient _http;
    private readonly string _model;
    private readonly string _apiKey;
    private readonly string _provider;

    public AiService(HttpClient http, IConfiguration config)
    {
        _http = http;
        _apiKey = config["Ai:ApiKey"]
            ?? throw new InvalidOperationException("Ai:ApiKey missing");
        _model = config["Ai:Model"] ?? "google/gemma-2-9b-it:free";

        _http.BaseAddress = new Uri(
            config["Ai:BaseUrl"] ?? "https://openrouter.ai/api/v1/");
    }

    public async Task<string> GetCoachFeedbackAsync(string question, string? userAnswer = null)
    {
        var system = """
            You are an expert technical interview coach for software engineering candidates.
            Be concise, practical, and encouraging.
            If the user gave an answer, give: (1) score out of 10, (2) what was good, (3) what to improve, (4) a model answer outline.
            If no user answer, give: key points to cover + a short model answer.
            """;

        var user = string.IsNullOrWhiteSpace(userAnswer)
            ? $"Interview question:\n{question}\n\nGive coaching guidance and a model answer outline."
            : $"Interview question:\n{question}\n\nCandidate answer:\n{userAnswer}\n\nEvaluate and coach.";

        return await ChatAsync(system, user);
    }

    public async Task<string> GetMockInterviewReplyAsync(List<ChatMessageDto> history, string userMessage)
    {
        var system = """
            You are a professional technical interviewer conducting a mock interview.
            Ask one clear question at a time. After 4-6 questions, give short overall feedback.
            Stay in character. Keep responses concise.
            """;

        var sb = new StringBuilder();
        sb.AppendLine(system);
        sb.AppendLine();
        foreach (var m in history)
            sb.AppendLine($"{m.Role}: {m.Content}");
        sb.AppendLine($"user: {userMessage}");

        return await ChatAsync("Follow the interview instructions above.", sb.ToString());
    }
    public async Task<string> GetAssistantReplyAsync(List<ChatMessageDto> history, string userMessage)
    {
        var system = """
        You are Quizora AI Assistant by HaMiko.
        Help students with IT topics: programming (C, C++, algorithms),
        OOP, DBMS, OS, networking, system design basics, and interview prep.
        Be clear, structured, and practical. Use short examples or code when useful.
        Answer in the same language the user writes in (Bangla or English).
        If the question is unclear, ask a short clarifying question.
        """;

        var sb = new StringBuilder();
        if (history != null)
        {
            foreach (var m in history.TakeLast(12)) // last 12 turns, cost control
            {
                var role = string.IsNullOrWhiteSpace(m.Role) ? "user" : m.Role.Trim().ToLowerInvariant();
                if (role is not ("user" or "assistant" or "system"))
                    role = "user";
                sb.AppendLine($"{role}: {m.Content}");
            }
        }

        sb.AppendLine($"user: {userMessage}");

        return await ChatAsync(system, sb.ToString());
    }
    public async Task<List<string>> GetWeakTopicsAsync(List<TopicScoreDto> history)
    {
        if (history == null || history.Count == 0)
            return new List<string> { "Start practicing any topic — no history yet." };

        var sb = new StringBuilder();
        foreach (var t in history)
        {
            var pct = t.Total == 0 ? 0 : (double)t.Score / t.Total * 100;
            sb.AppendLine($"{t.Topic}: {t.Score}/{t.Total} ({pct:0}%)");
        }

        var reply = await ChatAsync(
            "From the practice scores, list 3-5 weakest topics only, one per line, no extra text.",
            "Practice history:\n" + sb);

        return reply
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(l => l.TrimStart('-', '*', ' ', '\t', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '.'))
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .Take(5)
            .ToList();
    }

    private async Task<string> ChatAsync(string system, string user)
    {
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _apiKey);

        // OpenRouter optional but recommended
        if (!_http.DefaultRequestHeaders.Contains("HTTP-Referer"))
            _http.DefaultRequestHeaders.TryAddWithoutValidation(
                "HTTP-Referer", "https://quizora.local");
        if (!_http.DefaultRequestHeaders.Contains("X-Title"))
            _http.DefaultRequestHeaders.TryAddWithoutValidation(
                "X-Title", "Quizora");

        var body = new
        {
            model = _model,
            messages = new object[]
            {
            new { role = "system", content = system },
            new { role = "user", content = user }
            },
            temperature = 0.7
        };

        var response = await _http.PostAsJsonAsync("chat/completions", body);
        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception($"OpenRouter error {(int)response.StatusCode}: {json}");

        using var doc = JsonDocument.Parse(json);
        return doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString()
            ?.Trim() ?? "";
    }

}