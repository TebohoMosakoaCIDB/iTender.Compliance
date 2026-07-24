namespace iTender.Compliance.Infrastructure.Models
{
    public class SigningHubOptions
    {
        public const string SectionName = "SigningHub";

        public string BaseUrl { get; set; } = string.Empty;

        public string Username { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string ApiKey { get; set; } = string.Empty;

        public string ClientId { get; set; } = string.Empty;

        public string ClientSecret { get; set; } = string.Empty;

        public string CallbackUrl { get; set; } = string.Empty;
    }
}
