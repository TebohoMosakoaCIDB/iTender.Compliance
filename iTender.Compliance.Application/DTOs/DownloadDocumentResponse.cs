using System.Text.Json.Serialization;

namespace iTender.Compliance.Application.DTOs
{
    public class DownloadDocumentResponse
    {
        [JsonPropertyName("base64")]
        public string Base64 { get; set; } = string.Empty;
    }
}
