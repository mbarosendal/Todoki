using Microsoft.AspNetCore.Identity.Data;
using System.ComponentModel.DataAnnotations;

namespace Todoki.Data.ViewModels
{
    public class ChangePasswordRequest
    {
        [Required, EmailAddress]
        public string EmailAddress { get; set; }
        [Required]
        public string OldPassword { get; set; }
        [Required]
        public string NewPassword { get; set; }
    }
}
