using System.Net.Http.Json;
using TickTask.Client.Services;
using TickTask.Shared;

public class TaskApiService
{
    private readonly HttpClient _http;

    public TaskApiService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<TaskItem>> GetAllAsync(int? projectId = null)
    {
        var url = projectId.HasValue ? $"api/TaskItems?projectId={projectId}" : "api/TaskItems";
        return await _http.GetFromJsonAsync<List<TaskItem>>(url) ?? new List<TaskItem>();
    }

    public async Task<TaskItem?> GetByIdAsync(int id)
    {
        if (id == 0) return null; // don't call backend with ID 0
        return await _http.GetFromJsonAsync<TaskItem>($"api/TaskItems/{id}");
    }

    public async Task<bool> UpdateAsync(TaskItem task)
    {
        var response = await _http.PutAsJsonAsync($"api/TaskItems/{task.TaskItemId}", task);
        return response.IsSuccessStatusCode;
    }

    public async Task<TaskItem?> CreateAsync(TaskItem task)
    {
        var response = await _http.PostAsJsonAsync("api/TaskItems", task);
        return await response.Content.ReadFromJsonAsync<TaskItem>(); // now includes DB-assigned ID
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/TaskItems/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> SaveTaskOrderAsync(List<TaskItem> reorderedTasks)
    {
        var response = await _http.PutAsJsonAsync("api/TaskItems/reorder", reorderedTasks);
        return response.IsSuccessStatusCode;
    }
}
