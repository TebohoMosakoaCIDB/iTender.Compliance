using System.Text.Json.Serialization;

namespace iTender.Compliance.Application.DTOs
{
    public class UploadDocumentResponse
    {
        [JsonPropertyName("documentid")]
        public int DocumentId { get; set; }
    }
}
