using System.Net.Http.Json;
using System.Text.Json;
using Quizora.Web.Models.Trivia;

namespace Quizora.Web.Services;

public class QuizApiClient
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public QuizApiClient(HttpClient http) => _http = http;

    public async Task<(TriviaQuizStart? quiz, string? error)> StartAsync(
        int limit = 10,
        string category = "Linux",
        string difficulty = "Easy",
        IEnumerable<string>? excludeFingerprints = null)
    {
        try
        {
            var url =
                $"api/Practice/quizapi?limit={limit}" +
                $"&category={Uri.EscapeDataString(category)}" +
                $"&difficulty={Uri.EscapeDataString(difficulty)}";

            if (excludeFingerprints != null)
            {
                var parts = excludeFingerprints
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => Uri.EscapeDataString(x.Trim()))
                    .Take(80)
                    .ToList();

                if (parts.Count > 0)
                    url += $"&exclude={string.Join("|", parts)}";
            }

            var response = await _http.GetAsync(url);
            var json = await response.Content.ReadAsStringAsync();

            ApiResult<TriviaQuizStart>? res = null;
            try
            {
                res = JsonSerializer.Deserialize<ApiResult<TriviaQuizStart>>(json, JsonOpts);
            }
            catch { /* ignore */ }

            if (res != null)
            {
                if (!res.IsSuccess)
                    return (null, res.Message ?? $"API error ({(int)response.StatusCode})");
                return (res.Data, null);
            }

            return (null, $"HTTP {(int)response.StatusCode}: {Truncate(json, 300)}");
        }
        catch (Exception ex)
        {
            return (null, ex.Message);
        }
    }

    public async Task<(TriviaScore? score, string? error)> SubmitAsync(TriviaSubmit submit)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/Practice/quizapi/check-answers", submit);
            var json = await response.Content.ReadAsStringAsync();

            ApiResult<TriviaScore>? res = null;
            try
            {
                res = JsonSerializer.Deserialize<ApiResult<TriviaScore>>(json, JsonOpts);
            }
            catch { }

            if (res != null)
            {
                if (!res.IsSuccess)
                    return (null, res.Message ?? $"API error ({(int)response.StatusCode})");
                return (res.Data, null);
            }

            return (null, $"HTTP {(int)response.StatusCode}: {Truncate(json, 300)}");
        }
        catch (Exception ex)
        {
            return (null, ex.Message);
        }
    }

    private static string Truncate(string s, int max)
        => string.IsNullOrEmpty(s) || s.Length <= max ? s : s[..max] + "...";
}