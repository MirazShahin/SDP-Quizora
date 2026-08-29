using System.Net.Http.Json;
using System.Text.Json;

namespace Quizora.Web.Services;

public class CodeClient
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions Opts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public CodeClient(HttpClient http) => _http = http;

    public async Task<(CodeRunResult? result, string? error)> RunAsync(
        string language,
        string sourceCode,
        string? stdin = null,
        string? expectedOutput = null)
    {
        try
        {
            var body = new
            {
                language,
                sourceCode,
                stdin,
                expectedOutput
            };

            var response = await _http.PostAsJsonAsync("api/Code/run", body);
            var json = await response.Content.ReadAsStringAsync();

            var parsed = JsonSerializer.Deserialize<ApiResult<CodeRunResult>>(json, Opts);
            if (parsed == null)
                return (null, $"HTTP {(int)response.StatusCode}");

            if (!parsed.IsSuccess)
                return (null, parsed.Message ?? "Run failed");

            return (parsed.Data, null);
        }
        catch (Exception ex)
        {
            return (null, FriendlyError.Describe(ex));
        }
    }

    public class CodeRunResult
    {
        public bool Success { get; set; }
        public bool Compiled { get; set; }
        public bool TimedOut { get; set; }
        public bool Passed { get; set; }
        public string Status { get; set; } = "";
        public string Stdout { get; set; } = "";
        public string Stderr { get; set; } = "";
        public string CompileOutput { get; set; } = "";
        public int ExitCode { get; set; }
        public long TimeMs { get; set; }
    }

    public class ApiResult<T>
    {
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
    }
}