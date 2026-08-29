using Quizora.Application.Common;
using Quizora.Application.DTOs.Invitations;
using System.Net.Http.Json;

namespace Quizora.Web.Services;

public class InvitationService
{
    private readonly HttpClient _http;

    public InvitationService(HttpClient http)
    {
        _http = http;
    }

    public async Task<Result?> InviteCandidate(InviteCandidateDto dto)
    {
        try
        {
            // Correct: InvitationsController → POST api/Invitations
            var response = await _http.PostAsJsonAsync("api/Invitations", dto);

            if (response.Content == null || response.Content.Headers.ContentLength == 0)
            {
                if (response.IsSuccessStatusCode)
                    return Result.Success("Invitation sent successfully");

                return Result.Failure($"API Error {(int)response.StatusCode}: {response.ReasonPhrase}");
            }

            var result = await response.Content.ReadFromJsonAsync<Result>();
            if (result != null)
                return result;

            if (response.IsSuccessStatusCode)
                return Result.Success("Invitation sent successfully");

            var raw = await response.Content.ReadAsStringAsync();
            return Result.Failure($"API Error {(int)response.StatusCode}: {raw}");
        }
        catch (Exception ex)
        {
            return Result.Failure(FriendlyError.Describe(ex));
        }
    }

    public async Task<Result<List<InvitationDto>>?> GetMyInvitations()
    {
        try
        {
            var response = await _http.GetAsync("api/Invitations/my");

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return Result<List<InvitationDto>>.Failure(
                    $"API Error {(int)response.StatusCode}: {error}");
            }

            return await response.Content.ReadFromJsonAsync<Result<List<InvitationDto>>>();
        }
        catch (Exception ex)
        {
            return Result<List<InvitationDto>>.Failure(FriendlyError.Describe(ex));
        }
    }
    public async Task<Result<BulkInviteResultDto>?> BulkInvite(BulkInviteDto dto)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/Invitations/bulk", dto);
            var json = await response.Content.ReadAsStringAsync();
            return System.Text.Json.JsonSerializer.Deserialize<Result<BulkInviteResultDto>>(json,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (Exception ex)
        {
            return Result<BulkInviteResultDto>.Failure(FriendlyError.Describe(ex));
        }
    }
}