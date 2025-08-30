using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace TickTask.Client.Shared
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

        // GET
        public Task<T?> GetJsonAsync<T>(string url, CancellationToken token = default) =>
            _client.GetFromJsonAsync<T>(url, JsonOptions, token);

        // POST
        public async Task<T?> PostAsJsonAsync<T>(string url, T value, CancellationToken token = default)
        {
            var response = await _client.PostAsJsonAsync(url, value, JsonOptions, token);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<T>(JsonOptions, token);
        }

        // PUT
        public Task<HttpResponseMessage> PutAsJsonAsync<T>(string url, T value, CancellationToken token = default) =>
            _client.PutAsJsonAsync(url, value, JsonOptions, token);

        // DELETE
        public Task<HttpResponseMessage> DeleteAsync(string url, CancellationToken token = default) =>
            _client.DeleteAsync(url, token);
    }
}
