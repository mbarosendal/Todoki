using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTask.Shared
{
    public class Project
    {
        [Key]
        public int ProjectId { get; set; }

        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string Title { get; set; } = "";

        public string Description { get; set; } = "";

        [DataType(DataType.Date)]
        public DateTime DeadLine { get; set; } = DateTime.Now.AddDays(1);

        // Navigation property kept here only
        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();

        [Required]
        public int UserId { get; set; }
    }

    public class TaskItem
    {
        [Key]
        public int TaskItemId { get; set; }

        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string Name { get; set; } = "";

        public string Description { get; set; } = "";
        public bool IsActiveTask { get; set; } = false;
        public bool IsDone { get; set; } = false;

        public int EstimatedNumberOfPomodoros { get; set; } = 1;
        public int PomodorosRanOnTask { get; set; } = 0;

        //[Required]
        //public int ProjectId { get; set; }
    }

    public class TimerSettings
    {
        public bool IsAutoStart { get; set; } = false;
        public bool IsAutoStartAfterRestart { get; set; } = true;
        public bool AutomaticallyMarkDoneTasks { get; set; } = true;
        public bool AutomaticallyProceedToNextTaskAfterDone { get; set; } = true;
        public bool AutomaticallyClearDoneTasks { get; set; } = false;
        public bool EnableNotifications { get; set; } = false;
        public int NumberOfPomodorosRun { get; set; } = 0;
        [Range(1, 99, ErrorMessage = "Must be between 1 and 99")]
        public int RunsBeforeLongBreak { get; set; } = 4;
    }

    public class CountdownTimer
    {
        [Range(1, long.MaxValue, ErrorMessage = "Duration must be at least 1 minute")]
        public TimeSpan Duration { get; set; }
        public TimeSpan RemainingTime { get; set; }
        public bool IsRunning { get; set; } = false;
        [StringLength(200, MinimumLength = 1)]
        public string Text { get; set; } = "";
    }

    public class PomodoroTimer : CountdownTimer
    {
        public PomodoroTimer()
        {
            // For testing
            Duration = new TimeSpan(0, 0, 3);
            //Duration = TimeSpan.FromMinutes(25); // Default Pomodoro duration
        }
    }

    public class ShortBreakTimer : CountdownTimer
    {
        public ShortBreakTimer()
        {
            // For testing
            Duration = new TimeSpan(0, 0, 4);
            //Duration = TimeSpan.FromMinutes(5); // Default Short Break duration
        }
    }

    public class LongBreakTimer : CountdownTimer
    {
        public LongBreakTimer()
        {
            // For testing
            //Duration = new TimeSpan(0, 0, 5);
            Duration = TimeSpan.FromMinutes(15); // Default Long Break duration
        }
    }

    public class User
    {
        [Key]
        public int UserId { get; set; }
        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = "";
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string UserName { get; set; } = "";
        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = "";

        [InverseProperty("User")]
        public ICollection<Project> Projects { get; set; } = new List<Project>();
        // Needs to save users timer settings (durations)
        //public CountdownTimer CountDownTimer { get; set; } = new CountdownTimer();
    }
}
