using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Todoki.Client.Shared
{
    public class HttpClientWrapper
    {
        private readonly HttpClient _client;
        public readonly JsonSerializerOptions JsonOptions;

        public HttpClientWrapper(HttpClient client, JsonSerializerOptions jsonOptions)
        {
            _client = client;
            JsonOptions = jsonOptions;
        }

        public Task<T?> GetJsonAsync<T>(string url, CancellationToken token = default) =>
            _client.GetFromJsonAsync<T>(url, JsonOptions, token);

        public Task<HttpResponseMessage> PostAsJsonAsync<T>(string url, T value, CancellationToken token = default) =>
            _client.PostAsJsonAsync(url, value, JsonOptions, token);

        public async Task<TResponse?> PostAsJsonAsync<TRequest, TResponse>(string url, TRequest value, CancellationToken token = default)
        {
            var response = await _client.PostAsJsonAsync(url, value, JsonOptions, token);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions, token);
        }

        public Task<HttpResponseMessage> PutAsJsonAsync<T>(string url, T value, CancellationToken token = default) =>
            _client.PutAsJsonAsync(url, value, JsonOptions, token);

        public Task<HttpResponseMessage> DeleteAsync(string url, CancellationToken token = default) =>
            _client.DeleteAsync(url, token);

        public Task<T?> ReadFromJsonAsync<T>(HttpResponseMessage response, CancellationToken token = default) =>
            response.Content.ReadFromJsonAsync<T>(JsonOptions, token);
    }
}