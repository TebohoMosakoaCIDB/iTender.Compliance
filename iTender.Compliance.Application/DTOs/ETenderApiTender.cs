using System.Text.Json.Serialization;

namespace iTender.Compliance.Application.DTOs
{
    public class ETenderApiTender
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("tender_No")]
        public string TenderNumber { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("department")]
        public string Department { get; set; } = string.Empty;

        [JsonPropertyName("category")]
        public string Category { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("date_Published")]
        public DateTime? DatePublished { get; set; }

        [JsonPropertyName("closing_Date")]
        public DateTime? ClosingDate { get; set; }

        [JsonPropertyName("contactPerson")]
        public string ContactPerson { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("telephone")]
        public string Telephone { get; set; } = string.Empty;

        [JsonPropertyName("briefingVenue")]
        public string BriefingVenue { get; set; } = string.Empty;

        // If the API returns a direct URL, you can add it; otherwise, construct it.
        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;
    }
}
