namespace iTender.Compliance.Infrastructure.Models
{
    public class SigningHubOptions
    {
        public const string SectionName = "SigningHub";

        public string BaseUrl { get; set; } = string.Empty;

        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Scope { get; set; } = string.Empty;
        public string AuthenticationEndpoint { get; set; } = string.Empty;
    }
}
