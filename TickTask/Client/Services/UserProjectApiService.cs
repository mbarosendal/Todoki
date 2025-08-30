using System.Threading.Tasks;
using TickTask.Client.Shared;
using TickTask.Shared;

namespace TickTask.Client.Services
{
    public class UserProjectApiService
    {
        private readonly HttpClientWrapper _http;

        public UserProjectApiService(HttpClientWrapper http)
        {
            _http = http;
        }

        public async Task<ProjectDto> GetDefaultProjectAsync()
        {
            return await _http.GetJsonAsync<ProjectDto>("api/UserProject")
                   ?? new ProjectDto();
        }

        public async Task<ProjectDto?> CreateDefaultProjectAsync(ProjectDto project)
        {
            return await _http.PostAsJsonAsync<ProjectDto, ProjectDto>("api/UserProject", project);
        }

        public async Task<bool> UpdateDefaultProjectAsync(ProjectDto project)
        {
            var response = await _http.PutAsJsonAsync("api/UserProject", project);
            return response.IsSuccessStatusCode;
        }
    }
}
