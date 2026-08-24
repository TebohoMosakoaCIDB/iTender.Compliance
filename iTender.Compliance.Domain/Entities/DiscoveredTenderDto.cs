namespace iTender.Compliance.Domain.Entities
{
    public class DiscoveredTenderDto
    {
        public string TenderNumber { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string EmployerName { get; set; } = string.Empty;

        public string? ContactName { get; set; }

        public string? ContactEmail { get; set; }

        public string? ContactNumber { get; set; }

        public DateTime? AdvertisedDate { get; set; }

        public DateTime? ClosingDate { get; set; }

        public string Source { get; set; } = string.Empty;

        public string TenderUrl { get; set; } = string.Empty;

        public bool IsConstruction { get; set; }

        public string? ClassOfWorks { get; set; }

        public double Confidence { get; set; }
    }
}
