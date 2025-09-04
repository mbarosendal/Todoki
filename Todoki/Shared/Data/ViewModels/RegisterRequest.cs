using System.ComponentModel.DataAnnotations;

namespace Todoki.Data.ViewModels
{
    public class RegisterRequest
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string EmailAddress { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; }
    }
}
