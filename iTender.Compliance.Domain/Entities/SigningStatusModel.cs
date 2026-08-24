using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Domain.Entities
{
    public class SigningStatusModel
    {
        public string PackageId { get; set; } = string.Empty;

        public string DocumentId { get; set; } = string.Empty;

        public SignatureStatus Status { get; set; }

        public string? StatusDescription { get; set; }

        public string? SignedDocumentId { get; set; }

        public DateTime? RequestedOn { get; set; }

        public DateTime? SignedOn { get; set; }

        public bool IsCompleted =>
            Status == SignatureStatus.Completed;

        public bool IsRejected =>
            Status == SignatureStatus.Rejected;

        public bool IsPending =>
            Status == SignatureStatus.Pending;
    }
}
