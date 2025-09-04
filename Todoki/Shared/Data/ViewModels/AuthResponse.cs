namespace Todoki.Data.Shared.ViewModels
{
    public class AuthResponse
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; } 
        public DateTime ExpiresAt { get; set; }
        public bool IsSuccess { get; set; } = true;
        public string Error { get; set; }
    }
}
