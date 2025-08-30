using TickTask.Client.Enums;

namespace TickTask.Client.Services
{    public class TimerStateService
    {
        public TimerType CurrentTimerType { get; private set; } = TimerType.Pomodoro;

        public event Action? OnChange;

        public void SetTimerType(TimerType type)
        {
            CurrentTimerType = type;
            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }

}
