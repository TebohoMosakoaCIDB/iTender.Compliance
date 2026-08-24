namespace iTender.Compliance.Application.DTOs
{
    public class TenderDetailModel
    {
        public Guid Id { get; set; }

        public string TenderNumber { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Employer { get; set; } = string.Empty;

        public DateTime ClosingDate { get; set; }

        public string TenderUrl { get; set; } = string.Empty;
    }

    public class TenderDetailModelDto
    {
        public Guid Id { get; set; }

        public string TenderNumber { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string EmployerName { get; set; } = string.Empty;

        public string? ContactName { get; set; }

        public string? ContactEmail { get; set; }

        public string? ContactNumber { get; set; }

        public DateTime AdvertisedDate { get; set; }

        public DateTime ClosingDate { get; set; }

        public string Source { get; set; } = string.Empty;

        public string TenderUrl { get; set; } = string.Empty;

        public Guid TenderSyncId { get; set; }

        public bool IsConstruction { get; set; }

        public string? ClassOfWorks { get; set; }

        public DateTime? AwardedDate { get; set; }

        public decimal? AwardValue { get; set; }

        public string? WinningContractor { get; set; }

        public bool IsRegisteredOnRoP { get; set; }

        public DateTime? RoPRegistrationDate { get; set; }

        public TenderSyncDetailModel? TenderSync { get; set; }

        public Guid? ComplianceCaseId { get; set; }
    }
}
