using System.Collections.Generic;
using System.Threading.Tasks;
using TickTask.Shared;

namespace TickTask.Client.Services
{
    public interface ITaskApiService
    {
        Task<List<TaskItem>> GetAllAsync();
        Task<TaskItem?> GetByIdAsync(int id);
        Task<bool> UpdateAsync(TaskItem task);
        Task<TaskItem?> CreateAsync(TaskItem task);
        Task<bool> DeleteAsync(int id);
        Task<bool> SaveTaskOrderAsync(List<TaskItem> reorderedTasks);
    }

    public interface IUserSettingsApiService
    {
        Task<UserSettings> GetAsync();
        Task<bool> UpdateAsync(UserSettings userSettings);
        Task<UserSettings> CreateAsync(UserSettings userSettings);
        Task<bool> DeleteAsync();

    }

}

