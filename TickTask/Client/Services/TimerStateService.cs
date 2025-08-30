using Microsoft.Extensions.Logging;
using TickTask.Client.Enums;

namespace TickTask.Client.Services
{    public class TimerStateService
    {
        private readonly ILogger<TimerStateService> _logger;

        public TimerStateService(ILogger<TimerStateService> logger)
        {
            _logger = logger;
        }

        public TimerType CurrentTimerType { get; private set; } = TimerType.Pomodoro;

        public event Func<Task>? OnChange;
        private async Task NotifyStateChanged()
        {
            _logger.LogWarning($"NotifyStateChanged called, subscribers: {OnChange?.GetInvocationList().Length ?? 0}");
            if (OnChange != null)
                await OnChange.Invoke();
        }

        public async Task SetTimerType(TimerType type)
        {
            _logger.LogWarning("TimerState received timer change!");
            CurrentTimerType = type;
            await NotifyStateChanged();
        }

    }

}
