using System.Net.Http.Json;
using System.Threading.Tasks;
using TickTask.Shared;

namespace TickTask.Client.Services
{
    public class UserProjectApiService
    {
        private readonly HttpClient _http;

        public UserProjectApiService(IHttpClientFactory httpFactory)
        {
            _http = httpFactory.CreateClient("ServerAPI");
        }

        public async Task<ProjectDto> GetDefaultProjectAsync()
        {
            return await _http.GetFromJsonAsync<ProjectDto>("api/UserProject");
        }

        public async Task<ProjectDto> CreateDefaultProjectAsync(ProjectDto project)
        {
            var response = await _http.PostAsJsonAsync("api/UserProject", project);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ProjectDto>();
        }

        public async Task UpdateDefaultProjectAsync(ProjectDto project)
        {
            var response = await _http.PutAsJsonAsync("api/UserProject", project);
            response.EnsureSuccessStatusCode();
        }
    }
}
