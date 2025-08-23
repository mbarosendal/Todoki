using System.ComponentModel.DataAnnotations;

namespace TickTask.Data.ViewModels
{
    public class GetUserRequest
    {
        [Required, EmailAddress]
        public string EmailAddress { get; set; }
    }
}
