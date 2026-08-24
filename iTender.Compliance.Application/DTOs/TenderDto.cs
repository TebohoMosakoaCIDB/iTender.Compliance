namespace iTender.Compliance.Application.DTOs
{
    public class TenderDto
    {
        public Guid Id { get; set; }

        public string? EmployerTenderNumber { get; set; }

        public string? Title { get; set; }

        public string? EmployerName { get; set; }

        public DateTime? DateAdvertised { get; set; }

        public DateTime? ClosingDateTime { get; set; }

        public Guid? CreatedBy { get; set; }
        public Guid? ModifiedBy { get; set; }
        public DateTime? AwardedDate { get; set; }
        public bool IsRegisteredOnRoP { get; set; }
        public bool HasComplianceCase { get; set; }
        public string? ComplianceCaseStatus { get; set; }
        public List<ContactForTenderDto> ContactPerson { get; set; } = [];
    }

    public class TenderDetailDto
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

        public ComplianceCaseDetailModel? ComplianceCase { get; set; }
    }

    public class TenderSearchModel
    {
        public string? SearchText { get; set; }

        public string? Source { get; set; }

        public bool? IsConstruction { get; set; }

        public bool? IsRegisteredOnRoP { get; set; }

        public bool? HasBeenAwarded { get; set; }

        public bool? HasComplianceCase { get; set; }

        public DateTime? FromAdvertisedDate { get; set; }

        public DateTime? ToAdvertisedDate { get; set; }

        public DateTime? FromClosingDate { get; set; }

        public DateTime? ToClosingDate { get; set; }

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 20;
    }
}
