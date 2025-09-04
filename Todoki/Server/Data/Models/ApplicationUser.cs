using Microsoft.AspNetCore.Identity;
using Todoki.Shared;

namespace Todoki.Server.Data.Models
{
    public class ApplicationUser : IdentityUser
    {
        public ICollection<Project> Projects { get; set; } = new List<Project>();
        public UserSettings? Settings { get; set; }
    }
}
