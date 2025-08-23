using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using TickTask.Shared;

namespace TickTask.Client.Services
{
    public class UserSettingsApiService : IUserSettingsApiService
    {

        private readonly HttpClient _http;

        public UserSettingsApiService(IHttpClientFactory httpFactory)
        {
            _http = httpFactory.CreateClient("ServerAPI");
        }

        // ✔️ 
        public async Task<UserSettingsDto> GetAsync()
        {
            return await _http.GetFromJsonAsync<UserSettingsDto>("api/UserSettings")
                   ?? new UserSettingsDto();
        }

        // ✔️ 
        public async Task<bool> UpdateAsync(UserSettingsDto userSettings)
        {
            var response = await _http.PutAsJsonAsync($"api/UserSettings", userSettings);
            return response.IsSuccessStatusCode;
        }

        public async Task<UserSettingsDto?> CreateAsync(UserSettingsDto userSettings)
        {
            var response = await _http.PostAsJsonAsync("api/UserSettings", userSettings);
            return await response.Content.ReadFromJsonAsync<UserSettingsDto>();
        }

        public async Task<bool> DeleteAsync()
        {
            var response = await _http.DeleteAsync($"api/UserSettings");
            return response.IsSuccessStatusCode;
        }

    }
}
