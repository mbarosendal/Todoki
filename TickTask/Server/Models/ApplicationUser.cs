using Microsoft.AspNetCore.Identity;
using TickTask.Shared;

namespace TickTask.Server.Models
{
    public class ApplicationUser : IdentityUser
    {
        public UserSettings? Settings { get; set; }
    }
}
