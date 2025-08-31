using TickTask.Client.Shared;
using TickTask.Shared.Data;

namespace TickTask.Client.Services
{
    public interface ITaskApiService
    {
        Task<List<TaskItemDto>> GetAllAsync();
        Task<TaskItemDto?> GetByIdAsync(int id);
        Task<bool> UpdateAsync(TaskItemDto task);
        Task<TaskItemDto?> CreateAsync(TaskItemDto task);
        Task<bool> DeleteAsync(int id);
        Task<bool> SaveTaskOrderAsync(List<TaskItemDto> reorderedTasks);
    }

    public class TaskApiService
    {
        private readonly HttpClientWrapper _http;

        public TaskApiService(HttpClientWrapper http)
        {
            _http = http;
        }

        public Task<List<TaskItemDto>> GetAllAsync(int? projectId = null)
        {
            var url = projectId.HasValue ? $"api/TaskItems?projectId={projectId}" : "api/TaskItems";
            return _http.GetJsonAsync<List<TaskItemDto>>(url)
                        .ContinueWith(t => t.Result ?? new List<TaskItemDto>());
        }

        public Task<TaskItemDto?> GetByIdAsync(int id)
        {
            if (id == 0) return Task.FromResult<TaskItemDto?>(null);
            return _http.GetJsonAsync<TaskItemDto>($"api/TaskItems/{id}");
        }

        public async Task<bool> UpdateAsync(TaskItemDto task)
        {
            var response = await _http.PutAsJsonAsync("api/TaskItems/" + task.TaskItemId, task);
            return response.IsSuccessStatusCode;
        }

        public async Task<TaskItemDto?> CreateAsync(TaskItemDto task)
        {
            return await _http.PostAsJsonAsync<TaskItemDto, TaskItemDto>("api/TaskItems", task);
        }


        public async Task<bool> DeleteAsync(int id)
        {
            var response = await _http.DeleteAsync($"api/TaskItems/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> SaveTaskOrderAsync(List<TaskItemDto> reorderedTasks)
        {
            var response = await _http.PutAsJsonAsync("api/TaskItems/reorder", reorderedTasks);
            return response.IsSuccessStatusCode;
        }
    }
}
