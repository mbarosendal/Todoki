using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TickTask.Shared
{
    public class ProjectDto
    {
        public int ProjectId { get; set; }

        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string Title { get; set; } = "";

        public string Description { get; set; } = "";

        public DateTime DeadLine { get; set; } = DateTime.Now.AddDays(1);

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? UserId { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? GuestId { get; set; }

        public bool IsValid => !string.IsNullOrEmpty(UserId) ^ !string.IsNullOrEmpty(GuestId);
    }

    public class TaskItemDto
    {
        public int TaskItemId { get; set; }
        public int SortOrder { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Name { get; set; } = "";

        public string Description { get; set; } = "";
        public bool IsActiveTask { get; set; } = false;
        public bool IsDone { get; set; } = false;
        public int EstimatedNumberOfPomodoros { get; set; } = 1;
        public int PomodorosRanOnTask { get; set; } = 0;

        [Required]
        public int ProjectId { get; set; }
    }

    public class UserSettingsDto
    {
        public int UserSettingsId { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? UserId { get; set; } = "";

        public TimeSpan PomodoroDurationMinutes { get; set; } = TimeSpan.FromMinutes(25);
        public TimeSpan ShortBreakDurationMinutes { get; set; } = TimeSpan.FromMinutes(5);
        public TimeSpan LongBreakDurationMinutes { get; set; } = TimeSpan.FromMinutes(15);

        public string PomodoroText { get; set; } = "";
        public string ShortBreakText { get; set; } = "";
        public string LongBreakText { get; set; } = "";

        public bool HideTasks { get; set; } = false;
        public bool HideActiveTask { get; set; } = false;
        public bool IsAutoStart { get; set; } = false;
        public bool IsAutoStartAfterRestart { get; set; } = true;
        public bool AutomaticallyMarkDoneTasks { get; set; } = true;
        public bool AutomaticallyProceedToNextTaskAfterDone { get; set; } = true;
        public bool AutomaticallyClearDoneTasks { get; set; } = false;
        public bool EnableNotifications { get; set; } = false;

        public int NumberOfPomodorosRun { get; set; } = 0;

        [Range(1, 99)]
        public int RunsBeforeLongBreak { get; set; } = 4;
    }

    public class CountdownTimerDto
    {
        [Range(1, long.MaxValue)]
        public TimeSpan Duration { get; set; }

        public TimeSpan RemainingTime { get; set; }
        public bool IsRunning { get; set; } = false;

        [StringLength(200, MinimumLength = 1)]
        public string Text { get; set; } = "";
    }

    public class PomodoroTimerDto : CountdownTimerDto { }
    public class ShortBreakTimerDto : CountdownTimerDto { }
    public class LongBreakTimerDto : CountdownTimerDto { }
}
