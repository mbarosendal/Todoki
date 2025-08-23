using System.ComponentModel.DataAnnotations;

namespace TickTask.Data.ViewModels
{
    // https://www.linkedin.com/learning/asp-dot-net-core-token-based-authentication/refreshing-expired-tokens?autoSkip=true&resume=false&u=57075649
    public class TokenRequest
    {
        [Required]
        public string Token { get; set; }
        [Required]
        public string RefreshToken { get; set; }
    }
}
