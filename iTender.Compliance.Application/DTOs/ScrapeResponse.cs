using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace iTender.Compliance.Application.DTOs
{
    public class ScrapeResponse
    {
        public string Source { get; set; } = string.Empty;
        public string ScrapedOn { get; set; } = string.Empty;
        public int Count { get; set; }
        public List<ScrapedTenderDto> Tenders { get; set; } = new();
    }

    public class ScrapedTenderDto
    {
        [JsonPropertyName("external_id")] public int ExternalId { get; set; }
        [JsonPropertyName("tender_number")] public string TenderNumber { get; set; } = "";
        [JsonPropertyName("title")] public string Title { get; set; } = "";
        [JsonPropertyName("category")] public string? Category { get; set; }
        [JsonPropertyName("type")] public string? Type { get; set; }
        [JsonPropertyName("employer_name")] public string? EmployerName { get; set; }
        [JsonPropertyName("status")] public string? Status { get; set; }
        [JsonPropertyName("advertised_date")] public string? AdvertisedDate { get; set; }
        [JsonPropertyName("closing_date")] public string? ClosingDate { get; set; }
        [JsonPropertyName("province")] public string? Province { get; set; }
        [JsonPropertyName("contact_person")] public string? ContactPerson { get; set; }
        [JsonPropertyName("contact_email")] public string? ContactEmail { get; set; }
        [JsonPropertyName("contact_telephone")] public string? ContactTelephone { get; set; }
        [JsonPropertyName("conditions")] public string? Conditions { get; set; }
        [JsonPropertyName("briefing_session")] public bool BriefingSession { get; set; }
        [JsonPropertyName("briefing_compulsory")] public bool BriefingCompulsory { get; set; }
        [JsonPropertyName("briefing_date")] public string? BriefingDate { get; set; }
        [JsonPropertyName("briefing_venue")] public string? BriefingVenue { get; set; }
        [JsonPropertyName("e_submission")] public bool ESubmission { get; set; }
        [JsonPropertyName("tender_url")] public string? TenderUrl { get; set; }
    }
}
