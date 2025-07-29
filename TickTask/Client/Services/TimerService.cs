using TickTask.Client.Enums;
using TickTask.Shared;

namespace TickTask.Client.Services
{
    public class TimerService
    {
        private System.Threading.Timer? _timer;
        private TimerType? _activeType;
        private CountdownTimer? _activeTimer;

        public event Action? OnTimerUpdate;
        public event Action<bool>? OnTimerFinish;

        public void Start(TimerType type, CountdownTimer timer)
        {
            if (timer.IsRunning) return;

            timer.IsRunning = true;

            _activeType = type;
            _activeTimer = timer;

            _timer = new System.Threading.Timer(_ => Tick(), null, 1000, 1000);
        }

        public void Stop(CountdownTimer timer)
        {
            timer.IsRunning = false;
            _timer?.Dispose();
            _timer = null;
        }

        public void Reset(CountdownTimer timer)
        {
            Stop(timer);
            timer.RemainingTime = timer.Duration;
            timer.IsRunning = false;
            OnTimerUpdate?.Invoke();
        }

        private void Tick()
        {
            if (_activeTimer is null) return;

            if (_activeTimer.RemainingTime > TimeSpan.Zero)
            {
                _activeTimer.RemainingTime = _activeTimer.RemainingTime.Subtract(TimeSpan.FromSeconds(1));
                OnTimerUpdate?.Invoke();

                if (_activeTimer.RemainingTime == TimeSpan.Zero)
                {
                    Stop(_activeTimer);
                    Reset(_activeTimer);
                    OnTimerFinish?.Invoke(false);
                }
            }
            else
            {
                Stop(_activeTimer);
            }
        }

        public string CalculateEstimatedTimeOfTaskCompletion(PomodoroTimer activePomodoro, ShortBreakTimer shortBreakTimer, LongBreakTimer longBreakTimer, TaskItem activeTask, TimerSettings timerSettings)
        {
            var remainingCurrentPomodoroTime = activePomodoro.RemainingTime.TotalMinutes;
            var remainingPomodorosTime = (activeTask.EstimatedNumberOfPomodoros - activeTask.PomodorosRanOnTask - 1) * activePomodoro.Duration.TotalMinutes;

            var remainingPomodoros = activeTask.EstimatedNumberOfPomodoros - activeTask.PomodorosRanOnTask;
            var breaksNeeded = remainingPomodoros - 1;

            var longBreaksCount = 0;
            var currentTimerPosition = timerSettings.NumberOfPomodorosRun + 1;

            for (int i = 0; i < breaksNeeded; i++)
            {
                if ((currentTimerPosition + i) % timerSettings.RunsBeforeLongBreak == 0)
                    longBreaksCount++;
            }

            var shortBreaksCount = breaksNeeded - longBreaksCount;
            var shortBreaksTime = shortBreaksCount * shortBreakTimer.Duration.TotalMinutes;
            var longBreaksTime = longBreaksCount * longBreakTimer.Duration.TotalMinutes;

            var estimatedTime = DateTime.UtcNow + TimeSpan.FromMinutes(remainingCurrentPomodoroTime + remainingPomodorosTime + shortBreaksTime + longBreaksTime);

            return estimatedTime < DateTime.UtcNow ? "Overdue" : estimatedTime.ToLocalTime().ToString("HH:mm");
        }

    }
}
