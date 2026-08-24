using System.Net.Http.Headers;
using Blazored.LocalStorage;

namespace Quizora.Web.Services;

public class AuthorizedHandler : DelegatingHandler
{
    private readonly ILocalStorageService _localStorage;

    public AuthorizedHandler(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            var token = await _localStorage.GetItemAsStringAsync("authToken");
            if (string.IsNullOrWhiteSpace(token))
                token = await _localStorage.GetItemAsStringAsync("token");

            if (!string.IsNullOrWhiteSpace(token))
            {
                token = token.Trim().Trim('"');
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }
        catch
        {
            // JS interop not ready yet — ignore
        }

        return await base.SendAsync(request, cancellationToken);
    }
}