using System.ComponentModel.DataAnnotations;

namespace TickTask.Server.Data.Models
{
    public class UserSettings
    {
        [Key]
        public int UserSettingsId { get; set; }
        [Required]
        public string? UserId { get; set; }
        public ApplicationUser User { get; set; } = null!;
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
        public int RunsBeforeLongBreak { get; set; } = 4;
    }
}
