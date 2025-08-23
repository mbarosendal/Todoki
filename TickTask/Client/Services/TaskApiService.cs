using System.Net.Http.Json;
using TickTask.Client.Services;
using TickTask.Shared;

public class TaskApiService
{
    private readonly HttpClient _http;

    public TaskApiService(IHttpClientFactory httpFactory)
    {
        _http = httpFactory.CreateClient("ServerAPI");
    }

    public async Task<List<TaskItemDto>> GetAllAsync(int? projectId = null)
    {
        var url = projectId.HasValue ? $"api/TaskItems?projectId={projectId}" : "api/TaskItems";
        return await _http.GetFromJsonAsync<List<TaskItemDto>>(url) ?? new List<TaskItemDto>();
    }

    public async Task<TaskItemDto?> GetByIdAsync(int id)
    {
        if (id == 0) return null; // don't call backend with ID 0
        return await _http.GetFromJsonAsync<TaskItemDto>($"api/TaskItems/{id}");
    }

    public async Task<bool> UpdateAsync(TaskItemDto task)
    {
        var response = await _http.PutAsJsonAsync($"api/TaskItems/{task.TaskItemId}", task);
        return response.IsSuccessStatusCode;
    }

    public async Task<TaskItemDto?> CreateAsync(TaskItemDto task)
    {
        var response = await _http.PostAsJsonAsync("api/TaskItems", task);
        return await response.Content.ReadFromJsonAsync<TaskItemDto>();
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
