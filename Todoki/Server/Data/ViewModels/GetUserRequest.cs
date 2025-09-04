using System.ComponentModel.DataAnnotations;

namespace Todoki.Data.ViewModels
{
    public class GetUserRequest
    {
        [Required, EmailAddress]
        public string EmailAddress { get; set; }
    }
}
