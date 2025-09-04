using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Todoki.Server.Data.Models
{
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
}
