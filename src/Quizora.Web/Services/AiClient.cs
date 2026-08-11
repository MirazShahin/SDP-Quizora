using System.Net.Http.Json;
using System.Text.Json;

namespace Quizora.Web.Services;

public class AiClient
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions Opts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public AiClient(HttpClient http) => _http = http;

    public async Task<(string? text, string? error)> CoachAsync(string question, string? userAnswer = null)
    {
        try
        {
            var res = await _http.PostAsJsonAsync("api/ai/coach", new { question, userAnswer });
            var json = await res.Content.ReadAsStringAsync();
            var parsed = JsonSerializer.Deserialize<ApiResult<string>>(json, Opts);
            if (parsed?.IsSuccess == true)
                return (parsed.Data, null);
            return (null, parsed?.Message ?? $"HTTP {(int)res.StatusCode}");
        }
        catch (Exception ex)
        {
            return (null, ex.Message);
        }
    }

    public async Task<(string? text, string? error)> MockReplyAsync(List<ChatMsg> history, string message)
    {
        try
        {
            var res = await _http.PostAsJsonAsync("api/ai/mock-interview", new { history, message });
            var json = await res.Content.ReadAsStringAsync();
            var parsed = JsonSerializer.Deserialize<ApiResult<string>>(json, Opts);
            if (parsed?.IsSuccess == true)
                return (parsed.Data, null);
            return (null, parsed?.Message ?? $"HTTP {(int)res.StatusCode}");
        }
        catch (Exception ex)
        {
            return (null, ex.Message);
        }
    }

    public async Task<(PerformanceSummaryDto? data, string? error)> WeakTopicsAsync()
    {
        try
        {
            var res = await _http.GetAsync("api/ai/weak-topics");
            var json = await res.Content.ReadAsStringAsync();
            var parsed = JsonSerializer.Deserialize<ApiResult<PerformanceSummaryDto>>(json, Opts);
            if (parsed?.IsSuccess == true)
                return (parsed.Data, null);
            return (null, parsed?.Message ?? $"HTTP {(int)res.StatusCode}");
        }
        catch (Exception ex)
        {
            return (null, ex.Message);
        }
    }

    public class WeakTopicDto
    {
        public string Topic { get; set; } = "";
        public int Score { get; set; }
        public int Total { get; set; }
        public double Accuracy { get; set; }
        public string Severity { get; set; } = "";
        public string Source { get; set; } = "";
    }

    public class PerformanceSummaryDto
    {
        public int PracticeSessions { get; set; }
        public int MockSessions { get; set; }
        public double AvgAccuracy { get; set; }
        public int WeakCount { get; set; }
        public string? StrongestTopic { get; set; }
        public List<WeakTopicDto> WeakTopics { get; set; } = new();
    }

    public class ChatMsg
    {
        public string Role { get; set; } = "user";
        public string Content { get; set; } = "";
    }

    public class ApiResult<T>
    {
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
    }
}
