using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Todoki.Server.Data.Models
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
}
