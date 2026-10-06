using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Domain.Entities
{
    public class CaseLetter : BaseEntity
    {
        public Guid ComplianceCaseId { get; set; }

        public LetterType Type { get; set; }

        public int LetterNumber { get; set; }

        /// <summary>The client group this letter belongs to, if it was issued as a consolidated letter.</summary>
        public Guid? ClientGroupId { get; set; }

        /// <summary>Navigation to the client group.</summary>
        public virtual ClientGroup? ClientGroup { get; set; }

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

        // ---- NEW PROPERTY ----

        /// <summary>
        /// Links this letter to a specific finding (since one case may have multiple findings).
        /// </summary>
        public Guid? ComplianceFindingId { get; set; }

        // ---- Navigation ----

        public virtual ComplianceCase ComplianceCase { get; set; } = null!;

        public virtual SigningRequest? SigningRequest { get; set; }
    }
}
