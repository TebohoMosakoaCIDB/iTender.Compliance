using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Domain.Entities
{
    public class AGSAReferral : BaseEntity
    {
        public Guid ComplianceCaseId { get; set; }

        public string ReferralNumber { get; set; } = string.Empty;

        public DateTime ReferralDate { get; set; }

        public Guid? ReferredByUserId { get; set; }

        public string Reason { get; set; } = string.Empty;

        public string? Description { get; set; }

        public EnforcementReferralStatus Status { get; set; }

        public string? FileName { get; set; }

        public string? FilePath { get; set; }

        public DateTime? AgsaResponseDate { get; set; }

        public string? AgsaResponse { get; set; }

        public virtual ComplianceCase ComplianceCase { get; set; } = null!;
    }
}
