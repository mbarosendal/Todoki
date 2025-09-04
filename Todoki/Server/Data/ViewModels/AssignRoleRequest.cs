using System.ComponentModel.DataAnnotations;

namespace Todoki.Data.ViewModels
{
    public class AssignRoleRequest
    {
        [Required, EmailAddress]
        public string EmailAddress { get; set; }
        public string Role { get; set; }
    }
}
