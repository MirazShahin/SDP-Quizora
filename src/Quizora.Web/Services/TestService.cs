using Quizora.Application.Common;
using Quizora.Application.DTOs.Questions;
using Quizora.Application.DTOs.Tests;
using System.Net.Http.Json;

namespace Quizora.Web.Services;

public class TestService
{
    private readonly HttpClient _http;

    public TestService(HttpClient http)
    {
        _http = http;
    }

    public async Task<Result<List<TestDto>>?> GetMyTests()
    {
        try
        {
            var response = await _http.GetAsync("api/Tests");

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return Result<List<TestDto>>.Failure($"API Error {(int)response.StatusCode}: {errorContent}");
            }

            return await response.Content.ReadFromJsonAsync<Result<List<TestDto>>>();
        }
        catch (Exception ex)
        {
            return Result<List<TestDto>>.Failure(FriendlyError.Describe(ex));
        }
    }

    public async Task<Result<TestDto>?> CreateTest(CreateTestDto dto)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/Tests", dto);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return Result<TestDto>.Failure($"API Error {(int)response.StatusCode}: {errorContent}");
            }

            return await response.Content.ReadFromJsonAsync<Result<TestDto>>();
        }
        catch (Exception ex)
        {
            return Result<TestDto>.Failure(FriendlyError.Describe(ex));
        }
    }

    public async Task<Result?> AddQuestion(Guid testId, CreateQuestionDto dto)
    {
        try
        {
            var response = await _http.PostAsJsonAsync($"api/Tests/{testId}/questions", dto);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return Result.Failure($"API Error {(int)response.StatusCode}: {errorContent}");
            }

            return await response.Content.ReadFromJsonAsync<Result>();
        }
        catch (Exception ex)
        {
            return Result.Failure(FriendlyError.Describe(ex));
        }
    }
}