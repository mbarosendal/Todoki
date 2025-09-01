using Blazored.LocalStorage;
using System.Net.Http.Headers;

public class JwtAuthorizationMessageHandler : DelegatingHandler
{
    private readonly ILocalStorageService _localStorage;

    public JwtAuthorizationMessageHandler(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Skip adding auth header for logout endpoint - This prevents the expired JWT from being sent with logout requests while keeping it for all other API calls.
        if (request.RequestUri?.PathAndQuery.Contains("/logout") == true)
        {
            return await base.SendAsync(request, cancellationToken);
        }

        var token = await _localStorage.GetItemAsync<string>("authToken");
        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}