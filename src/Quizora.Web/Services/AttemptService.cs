using Quizora.Application.Common;
using Quizora.Application.DTOs.Attempts;
using Quizora.Application.DTOs.Questions;
using System.Net.Http.Json;

namespace Quizora.Web.Services;

public class AttemptService
{
    private readonly HttpClient _http;

    public AttemptService(HttpClient http)
    {
        _http = http;
    }

    // Backend: GET api/Attempts/{invitationId}/questions
    public async Task<Result<List<QuestionDto>>?> StartTest(Guid invitationId)
    {
        try
        {
            var response = await _http.GetAsync($"api/Attempts/{invitationId}/questions");

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return Result<List<QuestionDto>>.Failure(
                    $"API Error {(int)response.StatusCode}: {(string.IsNullOrWhiteSpace(error) ? response.ReasonPhrase : error)}");
            }

            return await response.Content.ReadFromJsonAsync<Result<List<QuestionDto>>>();
        }
        catch (Exception ex)
        {
            return Result<List<QuestionDto>>.Failure(FriendlyError.Describe(ex));
        }
    }

    // Backend: POST api/Attempts/{invitationId}/submit
    public async Task<Result<ResultDto>?> SubmitTest(Guid invitationId, SubmitTestDto dto)
    {
        try
        {
            var response = await _http.PostAsJsonAsync($"api/Attempts/{invitationId}/submit", dto);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return Result<ResultDto>.Failure(
                    $"API Error {(int)response.StatusCode}: {(string.IsNullOrWhiteSpace(error) ? response.ReasonPhrase : error)}");
            }

            return await response.Content.ReadFromJsonAsync<Result<ResultDto>>();
        }
        catch (Exception ex)
        {
            return Result<ResultDto>.Failure(FriendlyError.Describe(ex));
        }
    }
}