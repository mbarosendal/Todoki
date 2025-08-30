using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TickTask.Server.Data.Models
{
    public class Project
    {
        [Key]
        public int ProjectId { get; set; }
        [Required, StringLength(200)]
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime DeadLine { get; set; } = DateTime.Now.AddDays(1);
        public string? UserId { get; set; }
        public string? GuestId { get; set; }
        public ApplicationUser? User { get; set; } = null!;
        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    }

    public class TaskItem
    {
        [Key]
        public int TaskItemId { get; set; }
        public int SortOrder { get; set; }
        [Required, StringLength(100)]
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public bool IsActiveTask { get; set; } = false;
        public bool IsDone { get; set; } = false;
        public int EstimatedNumberOfPomodoros { get; set; } = 1;
        public int PomodorosRanOnTask { get; set; } = 0;
        [Required]
        public int ProjectId { get; set; }
        [ForeignKey("ProjectId")]
        public Project? Project { get; set; }
    }

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
