using System.Net.Http.Json;
using System.Threading.Tasks;
using TickTask.Shared;

namespace TickTask.Client.Services
{
    public class UserProjectApiService
    {
        private readonly HttpClient _httpClient;

        public UserProjectApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Project> GetDefaultProjectAsync()
        {
            return await _httpClient.GetFromJsonAsync<Project>("api/UserProject");
        }

        public async Task<Project> CreateDefaultProjectAsync(Project project)
        {
            var response = await _httpClient.PostAsJsonAsync("api/UserProject", project);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Project>();
        }

        public async Task UpdateDefaultProjectAsync(Project project)
        {
            var response = await _httpClient.PutAsJsonAsync("api/UserProject", project);
            response.EnsureSuccessStatusCode();
        }
    }
}
