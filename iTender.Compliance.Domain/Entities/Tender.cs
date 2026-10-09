using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Domain.Entities
{
    public class Tender : BaseEntity
    {
        public string TenderNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string NormalizedTitle { get; set; } = string.Empty;
        public string? Description { get; set; }

        public string EmployerName { get; set; } = string.Empty;

        public string? ContactName { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactNumber { get; set; }

        public DateTime AdvertisedDate { get; set; }
        public DateTime ClosingDate { get; set; }

        public string Source { get; set; } = string.Empty;
        public string TenderUrl { get; set; } = string.Empty;

        // eTenders information
        public string? ExternalId { get; set; }
        public string? Ocid { get; set; }
        public string? ProcurementCategory { get; set; }
        public string? Province { get; set; }
        public string? DeliveryLocation { get; set; }
        public string? ProcurementMethod { get; set; }
        public string? ProcurementMethodDetails { get; set; }
        public string? TenderStatus { get; set; }
        public string? SpecialConditions { get; set; }
        public string? SourceDocumentUrl { get; set; }

        public Guid TenderSyncId { get; set; }

        public bool IsConstruction { get; set; }
        public string? ClassOfWorks { get; set; }
        public ClassOfWorksSource ClassOfWorksSource { get; set; } = ClassOfWorksSource.NotSpecified;
        public string? ClaimedClassOfWorks { get; set; }
        public DateTime? AwardedDate { get; set; }
        public decimal? AwardValue { get; set; }
        public string? WinningContractor { get; set; }

        public bool IsRegisteredOnRoP { get; set; }
        public DateTime? RoPRegistrationDate { get; set; }

        public virtual TenderSync TenderSync { get; set; } = null!;
        public virtual ComplianceCase? ComplianceCase { get; set; }

        /// <summary>Who last confirmed or overrode the automated construction classification.</summary>
        public Guid? ClassificationReviewedByUserId { get; set; }

        /// <summary>When the classification was reviewed.</summary>
        public DateTime? ClassificationReviewedOn { get; set; }

        /// <summary>Free-text reason when a CO overrides the automated classification.</summary>
        public string? ClassificationReviewNotes { get; set; }

        /// <summary>True if the automated classification was changed by a CO.</summary>
        public bool ClassificationOverridden { get; set; }
        /// <summary>
        /// Whether the tender was advertised on the Board's i-Tender portal.
        /// Populated during compliance processing from the Advertisement finding.
        /// </summary>
        public bool IsAdvertised { get; set; }

        /// <summary>
        /// Canonical, normalised employer name used for client grouping.
        /// Derived from EmployerName on ingestion or on first group creation.
        /// </summary>
        public string NormalizedEmployerName { get; set; } = string.Empty;
    }
}
