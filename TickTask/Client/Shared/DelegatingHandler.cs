using Blazored.LocalStorage;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using TickTask.Shared.Data.ViewModels;

public class JwtAuthorizationMessageHandler : DelegatingHandler
{
    private readonly ILocalStorageService _localStorage;
    private readonly IHttpClientFactory _httpClientFactory;
    private static readonly SemaphoreSlim _refreshSemaphore = new(1, 1);
    private readonly string _baseAddress;

    public JwtAuthorizationMessageHandler(ILocalStorageService localStorage, IHttpClientFactory httpClientFactory, string baseAddress)
    {
        _localStorage = localStorage;
        _httpClientFactory = httpClientFactory;
        _baseAddress = baseAddress;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Skip adding auth header for logout endpoint
        if (request.RequestUri?.PathAndQuery.Contains("/logout") == true)
        {
            return await base.SendAsync(request, cancellationToken);
        }

        var token = await _localStorage.GetItemAsync<string>("authToken");
        if (!string.IsNullOrWhiteSpace(token))
        {
            if (IsTokenExpired(token))
            {
                token = await RefreshTokenAsync();
            }

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }

    private bool IsTokenExpired(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();

            var jwt = handler.ReadJwtToken(token);

            // Check if token expires within the next 5 minutes
            var expiryTime = jwt.ValidTo;
            var currentTime = DateTime.UtcNow;
            var bufferTime = TimeSpan.FromMinutes(5);

            return expiryTime <= currentTime.Add(bufferTime);
        }
        catch
        {
            // If we can't parse the token, consider it expired
            return true;
        }
    }

    private async Task<string?> RefreshTokenAsync()
    {
        // Use semaphore to prevent multiple simultaneous refresh attempts
        await _refreshSemaphore.WaitAsync();
        try
        {
            var refreshToken = await _localStorage.GetItemAsync<string>("refreshToken");
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return null;
            }

            // Create a new HttpClient without the message handler to avoid infinite loop
            using var httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri(_baseAddress);

            var tokenRequest = new
            {
                Token = await _localStorage.GetItemAsync<string>("authToken"),
                RefreshToken = refreshToken
            };

            var response = await httpClient.PostAsJsonAsync("api/authentication/refresh-token", tokenRequest);

            if (response.IsSuccessStatusCode)
            {
                var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();

                if (authResponse?.IsSuccess == true && !string.IsNullOrWhiteSpace(authResponse.Token))
                {
                    // Store the new tokens
                    await _localStorage.SetItemAsync("authToken", authResponse.Token);
                    await _localStorage.SetItemAsync("refreshToken", authResponse.RefreshToken);

                    return authResponse.Token;
                }
            }

            // If refresh failed, clear tokens
            await _localStorage.RemoveItemAsync("authToken");
            await _localStorage.RemoveItemAsync("refreshToken");

            return null;
        }
        catch
        {
            // On any error, clear tokens
            await _localStorage.RemoveItemAsync("authToken");
            await _localStorage.RemoveItemAsync("refreshToken");
            return null;
        }
        finally
        {
            _refreshSemaphore.Release();
        }
    }
}