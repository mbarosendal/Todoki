using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using TickTask.Shared;

namespace TickTask.Client.Services
{
    public class UserSettingsApiService : IUserSettingsApiService
    {

        private readonly HttpClient _http;

        public UserSettingsApiService(HttpClient http)
        {
            _http = http;
        }

        // ✔️ 
        public async Task<UserSettings> GetAsync()
        {
            return await _http.GetFromJsonAsync<UserSettings>("api/UserSettings")
                   ?? new UserSettings();
        }

        // ✔️ 
        public async Task<bool> UpdateAsync(UserSettings userSettings)
        {
            var response = await _http.PutAsJsonAsync($"api/UserSettings", userSettings);
            return response.IsSuccessStatusCode;
        }

        public async Task<UserSettings?> CreateAsync(UserSettings userSettings)
        {
            var response = await _http.PostAsJsonAsync("api/UserSettings", userSettings);
            return await response.Content.ReadFromJsonAsync<UserSettings>();
        }

        public async Task<bool> DeleteAsync()
        {
            var response = await _http.DeleteAsync($"api/UserSettings");
            return response.IsSuccessStatusCode;
        }

    }
}
