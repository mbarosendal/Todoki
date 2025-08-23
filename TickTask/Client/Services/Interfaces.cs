using System.Collections.Generic;
using System.Threading.Tasks;
using TickTask.Shared;

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

    public interface IUserSettingsApiService
    {
        Task<UserSettingsDto> GetAsync();
        Task<bool> UpdateAsync(UserSettingsDto userSettings);
        Task<UserSettingsDto> CreateAsync(UserSettingsDto userSettings);
        Task<bool> DeleteAsync();

    }

}

