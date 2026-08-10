using Intersoft.Crosslight.Mobile;
using Microsoft.AspNetCore.Components.Authorization;
using Quizora.Application.Common;
using Quizora.Application.DTOs.Auth;
using Quizora.Web.Auth;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Blazored.LocalStorage;
using ILocalStorageService = Blazored.LocalStorage.ILocalStorageService;
namespace Quizora.Web.Services;

public class AuthService
{
    private readonly HttpClient _http;
    private readonly ILocalStorageService _localStorage;
    private readonly AuthenticationStateProvider _authStateProvider;

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

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return Result<AuthResponseDto>.Failure($"Error: {response.StatusCode} - {errorContent}");
            }

            return await response.Content.ReadFromJsonAsync<Result<AuthResponseDto>>();
        }
        catch (Exception ex)
        {
            return Result<AuthResponseDto>.Failure($"Exception: {ex.Message}");
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
                return Result<AuthResponseDto>.Failure($"Error {response.StatusCode}: {content}");
            }

            var result = System.Text.Json.JsonSerializer.Deserialize<Result<AuthResponseDto>>(content, new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (result != null && result.IsSuccess && result.Data != null)
            {
                await _localStorage.SetItemAsStringAsync("authToken", result.Data.Token);
                ((CustomAuthStateProvider)_authStateProvider).NotifyUserAuthentication(result.Data.Token);
                _http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", result.Data.Token);
            }

            return result;
        }
        catch (Exception ex)
        {
            return Result<AuthResponseDto>.Failure($"Exception: {ex.Message}");
        }
    }

    public async Task Logout()
    {
        await _localStorage.RemoveItemAsync("authToken");
        ((CustomAuthStateProvider)_authStateProvider).NotifyUserLogout();
        _http.DefaultRequestHeaders.Authorization = null;
    }
}