using TickTask.Shared;

namespace TickTask.Server.Services
{
    public class TimerService
    {
        private System.Threading.Timer? _timer;
        private CountdownTimer _currentTimer;
        public event Action? OnTimerUpdate;

        public void Start(CountdownTimer timer)
        {
            if (timer.IsTimerRunning) return;

            timer.IsTimerRunning = true;
            timer.RemainingTime = timer.Duration;
            _currentTimer = timer;

            _timer = new System.Threading.Timer(Tick, null, 1000, 1000);
        }

        public void Stop(CountdownTimer timer)
        {
            timer.IsTimerRunning = false;
            _timer?.Dispose();
        }

        public void Reset(CountdownTimer timer)
        {
            Stop(timer);
            timer.RemainingTime = timer.Duration;
            OnTimerUpdate?.Invoke();
        }

        private void Tick(object? state)
        {
            if (_currentTimer.RemainingTime > TimeSpan.Zero)
            {
                _currentTimer.RemainingTime = _currentTimer.RemainingTime.Subtract(TimeSpan.FromSeconds(1));
                OnTimerUpdate?.Invoke();
            }
            else
            {
                Stop(_currentTimer);
            }
        }
    }
}
