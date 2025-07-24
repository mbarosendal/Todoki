using System.Net.Http.Json;
using TickTask.Shared;

public class TaskService
{
    private readonly HttpClient _http;

    public TaskService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<TaskItem>> GetAllAsync()
    {
        return await _http.GetFromJsonAsync<List<TaskItem>>("api/TaskItems")
               ?? new List<TaskItem>();
    }

    public async Task<TaskItem?> GetByIdAsync(int id)
    {
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
        return await response.Content.ReadFromJsonAsync<TaskItem>();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/TaskItems/{id}");
        return response.IsSuccessStatusCode;
    }
}
