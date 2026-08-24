using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Domain.Entities
{
    public class SigningRequest : BaseEntity
    {

        public Guid CaseLetterId { get; set; }

        public SigningRequestStatus Status { get; set; }

        // SigningHub IDs

        public string? DocumentId { get; set; }
        public int? PackageId { get; set; }

        // Files
        public string OriginalDocumentPath { get; set; } = string.Empty;

        public string? SignedDocumentPath { get; set; }

        // Signing
        public Guid? SignerId { get; set; }

        public string? SignerName { get; set; }

        public string? SignerEmail { get; set; }

        public DateTime? SentOn { get; set; }

        public DateTime? SignedOn { get; set; }

        public string? FailureReason { get; set; }

        public string FileName { get; set; } = string.Empty;

        #region Navigation

        public virtual CaseLetter CaseLetter { get; set; } = null!;

        #endregion
    }
}
