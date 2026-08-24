namespace iTender.Compliance.Domain.Entities
{
    public class Tender : BaseEntity
    {
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
        public virtual TenderSync TenderSync { get; set; } = null!;
        public virtual ComplianceCase? ComplianceCase { get; set; }
    }
}
