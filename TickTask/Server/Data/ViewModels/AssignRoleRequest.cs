using System.ComponentModel.DataAnnotations;

namespace TickTask.Data.ViewModels
{
    public class AssignRoleRequest
    {
        [Required, EmailAddress]
        public string EmailAddress { get; set; }
        public string Role { get; set; }
    }
}
