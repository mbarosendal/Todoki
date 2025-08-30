using System.Net.Http.Json;
using System.Text.Json;

namespace TickTask.Client.Shared
{
    public class HttpClientWrapper
    {
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _jsonOptions;

        public HttpClientWrapper(HttpClient client, JsonSerializerOptions jsonOptions)
        {
            _client = client;
            _jsonOptions = jsonOptions;
        }

        public Task<T?> GetFromJsonAsync<T>(string url, CancellationToken token = default) =>
            _client.GetFromJsonAsync<T>(url, _jsonOptions, token);

        public Task<HttpResponseMessage> PostAsJsonAsync<T>(string url, T value, CancellationToken token = default) =>
            _client.PostAsJsonAsync(url, value, _jsonOptions, token);

        public Task<HttpResponseMessage> PutAsJsonAsync<T>(string url, T value, CancellationToken token = default) =>
            _client.PutAsJsonAsync(url, value, _jsonOptions, token);

    }

}
