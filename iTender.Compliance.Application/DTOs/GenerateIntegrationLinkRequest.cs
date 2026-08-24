using System.Text.Json.Serialization;

namespace iTender.Compliance.Application.DTOs
{
    public class GenerateIntegrationLinkRequest
    {
        [JsonPropertyName("package_id")]
        public int PackageId { get; set; }

        [JsonPropertyName("language")]
        public string Language { get; set; } = "en-US";

        [JsonPropertyName("user_email")]
        public string UserEmail { get; set; } = string.Empty;

        [JsonPropertyName("callback_url")]
        public string CallbackUrl { get; set; } = string.Empty;

        [JsonPropertyName("collapse_panels")]
        public bool CollapsePanels { get; set; } = true;
    }
}
