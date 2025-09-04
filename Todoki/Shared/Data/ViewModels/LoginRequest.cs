using System.ComponentModel.DataAnnotations;

namespace Todoki.Data.Shared.ViewModels
{
    public class LoginRequest
    {
        [Required, EmailAddress]
        public string EmailAddress { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
