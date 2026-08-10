using Quizora.Application.Common;
using System.Net.Http.Json;

namespace Quizora.Web.Services;

public class InterviewService
{
    private readonly HttpClient _http;

    public InterviewService(HttpClient http)
    {
        _http = http;
    }

    public async Task<Result<object>?> GetTopics()
    {
        return await _http.GetFromJsonAsync<Result<object>>("api/Interview/topics");
    }

    public async Task<Result<object>?> GetQAs(Guid topicId)
    {
        return await _http.GetFromJsonAsync<Result<object>>($"api/Interview/topics/{topicId}/qas");
    }
}