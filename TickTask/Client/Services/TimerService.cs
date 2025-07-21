using TickTask.Shared;

namespace TickTask.Server.Services
{
    public class TimerService
    {
        private System.Threading.Timer? _timer;
        private CountdownTimer _currentTimer;
        public event Action? OnTimerUpdate;
        public event Action? OnTimerFinish;

        public void Start(CountdownTimer timer)
        {
            //if (_timer is not null) _timer.Dispose();
            if (timer.IsRunning) return;

            timer.IsRunning = true;

            // if timer was not previously paused, start timer anew
            if (timer.PausedTime == TimeSpan.Zero)
                timer.PausedTime = timer.Duration;

            // otherwise start timer from paused time
            timer.RemainingTime = timer.PausedTime;

            _currentTimer = timer;
            _timer = new System.Threading.Timer(Tick, null, 1000, 1000);
        }

        public void Stop(CountdownTimer timer)
        {
            timer.IsRunning = false;
            timer.PausedTime = timer.RemainingTime;
            _timer?.Dispose();
        }

        public void Reset(CountdownTimer timer)
        {
            timer.RemainingTime = timer.Duration;
            timer.PausedTime = TimeSpan.Zero;
            timer.IsRunning = false;
            Stop(timer);
            OnTimerUpdate?.Invoke();
        }

        private void Tick(object? state)
        {
            if (_currentTimer.RemainingTime > TimeSpan.Zero)
            {
                _currentTimer.RemainingTime = _currentTimer.RemainingTime.Subtract(TimeSpan.FromSeconds(1));
                OnTimerUpdate?.Invoke();

                // when timer finishes
                if (_currentTimer.RemainingTime == TimeSpan.Zero)
                {
                    Stop(_currentTimer);
                    Reset(_currentTimer);
                    OnTimerFinish?.Invoke();
                }
            }
            else
            {
                Stop(_currentTimer);
            }
        }
    }
}
