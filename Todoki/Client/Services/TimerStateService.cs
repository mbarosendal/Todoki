using Microsoft.Extensions.Logging;
using Todoki.Client.Enums;

namespace Todoki.Client.Services
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
            if (OnChange != null)
                await OnChange.Invoke();
        }

        public async Task SetTimerType(TimerType type)
        {
            CurrentTimerType = type;
            await NotifyStateChanged();
        }

    }

}
