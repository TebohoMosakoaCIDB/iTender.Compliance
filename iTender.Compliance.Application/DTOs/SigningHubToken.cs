namespace iTender.Compliance.Application.DTOs
{
    public class SigningHubToken
    {
        public string AccessToken { get; set; } = string.Empty;

        public string RefreshToken { get; set; } = string.Empty;

        public string TokenType { get; set; } = string.Empty;

        public int ExpiresIn { get; set; }
    }
}
