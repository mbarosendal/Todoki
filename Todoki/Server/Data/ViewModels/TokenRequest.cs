using System.ComponentModel.DataAnnotations;

namespace Todoki.Data.ViewModels
{
   public class TokenRequest
    {
        [Required]
        public string Token { get; set; }
        [Required]
        public string RefreshToken { get; set; }
    }
}
