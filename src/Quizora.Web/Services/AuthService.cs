using Microsoft.AspNetCore.Components.Authorization;
using Quizora.Application.Common;
using Quizora.Application.DTOs.Auth;
using Quizora.Web.Auth;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Blazored.LocalStorage;

namespace Quizora.Web.Services;

public class AuthService
{
    private readonly HttpClient _http;
    private readonly ILocalStorageService _localStorage;
    private readonly AuthenticationStateProvider _authStateProvider;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public AuthService(HttpClient http, ILocalStorageService localStorage, AuthenticationStateProvider authStateProvider)
    {
        _http = http;
        _localStorage = localStorage;
        _authStateProvider = authStateProvider;
    }

    public async Task<Result<AuthResponseDto>?> Register(RegisterDto dto)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/Auth/register", dto);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return Result<AuthResponseDto>.Failure(ApiErrors.Friendly(
                    $"Error: {response.StatusCode} - {content}",
                    "Registration failed. Please try again."));

            try
            {
                return JsonSerializer.Deserialize<Result<AuthResponseDto>>(content, JsonOpts);
            }
            catch
            {
                return Result<AuthResponseDto>.Failure(ApiErrors.Friendly(content, "Registration failed."));
            }
        }
        catch (Exception ex)
        {
            return Result<AuthResponseDto>.Failure(ApiErrors.Friendly(ex.Message, "Cannot reach server."));
        }
    }

    public async Task<Result<AuthResponseDto>?> Login(LoginDto dto)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/Auth/login", dto);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return Result<AuthResponseDto>.Failure(ApiErrors.Friendly(
                    $"Error {response.StatusCode}: {content}",
                    "Login failed. Please try again."));
            }

            Result<AuthResponseDto>? result;
            try
            {
                result = JsonSerializer.Deserialize<Result<AuthResponseDto>>(content, JsonOpts);
            }
            catch
            {
                return Result<AuthResponseDto>.Failure(ApiErrors.Friendly(content, "Login failed."));
            }

            if (result != null && result.IsSuccess && result.Data != null)
            {
                await _localStorage.SetItemAsStringAsync("authToken", result.Data.Token);
                ((CustomAuthStateProvider)_authStateProvider).NotifyUserAuthentication(result.Data.Token);
                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", result.Data.Token);
            }
            else if (result != null && !result.IsSuccess)
            {
                result = Result<AuthResponseDto>.Failure(
                    ApiErrors.Friendly(result.Message, result.Message ?? "Login failed"));
            }

            return result;
        }
        catch (Exception ex)
        {
            return Result<AuthResponseDto>.Failure(ApiErrors.Friendly(ex.Message, "Cannot reach server."));
        }
    }

    public async Task Logout()
    {
        await _localStorage.RemoveItemAsync("authToken");
        ((CustomAuthStateProvider)_authStateProvider).NotifyUserLogout();
        _http.DefaultRequestHeaders.Authorization = null;
    }
}
