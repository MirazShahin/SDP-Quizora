using Blazored.LocalStorage;
using System.Net.Http.Headers;

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

            if (!string.IsNullOrWhiteSpace(token))
            {
                token = token.Trim().Trim('"'); // extra quotes সরানো
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }
        catch
        {
            // prerender — ignore
        }

        return await base.SendAsync(request, cancellationToken);
    }
}