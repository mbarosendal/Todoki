using System.ComponentModel.DataAnnotations;

namespace TickTask.Data.ViewModels
{
    public class LoginRequest
    {
        // https://www.linkedin.com/learning/asp-dot-net-core-token-based-authentication/logging-in-users?autoSkip=true&resume=false&u=57075649
        [Required, EmailAddress]
        public string EmailAddress { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
