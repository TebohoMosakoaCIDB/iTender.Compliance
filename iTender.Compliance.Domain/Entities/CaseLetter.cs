using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Domain.Entities
{
    public class CaseLetter : BaseEntity
    {
        public Guid ComplianceCaseId { get; set; }

        public LetterType Type { get; set; }

        public int LetterNumber { get; set; }

        public string RecipientName { get; set; } = string.Empty;

        public string RecipientEmail { get; set; } = string.Empty;

        public string FileName { get; set; } = string.Empty;

        public string FilePath { get; set; } = string.Empty;

        public bool EmailSent { get; set; }

        public DateTime SentOn { get; set; }

        public DateTime ResponseDueOn { get; set; }

        public DateTime? RespondedOn { get; set; }

        public bool? Accepted { get; set; }

        public string? ResponseComments { get; set; }

        public virtual ComplianceCase ComplianceCase { get; set; } = null!;

        public virtual SigningRequest? SigningRequest { get; set; }
    }
}
