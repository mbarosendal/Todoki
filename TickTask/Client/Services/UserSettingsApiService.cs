using TickTask.Client.Shared;
using TickTask.Shared.Data;

namespace TickTask.Client.Services
{
    public interface IUserSettingsApiService
    {
        Task<UserSettingsDto> GetAsync();
        Task<bool> UpdateAsync(UserSettingsDto userSettings);
        Task<UserSettingsDto> CreateAsync(UserSettingsDto userSettings);
        Task<bool> DeleteAsync();
    }

    public class UserSettingsApiService : IUserSettingsApiService
    {
        private readonly HttpClientWrapper _http;

        public UserSettingsApiService(HttpClientWrapper http)
        {
            _http = http;
        }

        public async Task<UserSettingsDto> GetAsync()
        {
            return await _http.GetJsonAsync<UserSettingsDto>("api/UserSettings")
                   ?? new UserSettingsDto();
        }

        public async Task<bool> UpdateAsync(UserSettingsDto userSettings)
        {
            var response = await _http.PutAsJsonAsync("api/UserSettings", userSettings);
            return response.IsSuccessStatusCode;
        }

        public async Task<UserSettingsDto?> CreateAsync(UserSettingsDto userSettings)
        {
            return await _http.PostAsJsonAsync<UserSettingsDto, UserSettingsDto>("api/UserSettings", userSettings);
        }

        public async Task<bool> DeleteAsync()
        {
            var response = await _http.DeleteAsync("api/UserSettings");
            return response.IsSuccessStatusCode;
        }
    }
}
