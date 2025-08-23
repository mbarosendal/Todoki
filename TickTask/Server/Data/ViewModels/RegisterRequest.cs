using System.ComponentModel.DataAnnotations;

namespace TickTask.Data.ViewModels
{
    public class RegisterRequest
    {
        // https://www.linkedin.com/learning/asp-dot-net-core-token-based-authentication/registering-new-users-using-usermanager?autoSkip=true&resume=false&u=57075649
        //public string FirstName { get; set; }
        //public string LastName { get; set; }
        [Required, EmailAddress]
        public string EmailAddress { get; set; }
        //[Required]
        //public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
        // https://www.linkedin.com/learning/asp-dot-net-core-token-based-authentication/adding-role-claims-to-tokens?autoSkip=true&resume=false&u=57075649 
        //[Required]
        //public string Role { get; set; }
    }
}
