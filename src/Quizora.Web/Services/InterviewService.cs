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
        try
        {
            return await _http.GetFromJsonAsync<Result<object>>("api/Interview/topics");
        }
        catch (Exception ex)
        {
            return Result<object>.Failure(FriendlyError.Describe(ex));
        }
    }

    public async Task<Result<object>?> GetQAs(Guid topicId)
    {
        try
        {
            return await _http.GetFromJsonAsync<Result<object>>($"api/Interview/topics/{topicId}/qas");
        }
        catch (Exception ex)
        {
            return Result<object>.Failure(FriendlyError.Describe(ex));
        }
    }
}