using System.Text.Json.Serialization;

namespace iTender.Compliance.Application.DTOs
{
    public class AddSignerRequest
    {
        [JsonPropertyName("user_email")]
        public string UserEmail { get; set; } = string.Empty;

        [JsonPropertyName("user_name")]
        public string UserName { get; set; } = string.Empty;

        [JsonPropertyName("role")]
        public string Role { get; set; } = "SIGNER";

        [JsonPropertyName("email_notification")]
        public bool EmailNotification { get; set; } = true;

        [JsonPropertyName("signing_order")]
        public int SigningOrder { get; set; } = 1;
    }
}
