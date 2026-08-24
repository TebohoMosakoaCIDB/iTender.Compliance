using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Domain.Entities
{
    public class ComplianceCase : BaseEntity
    {
        public Guid TenderId { get; set; }
        public Guid? AgentId { get; set; }
        public CaseStatus Status { get; set; }
        public ComplianceOutcome? Outcome { get; set; }
        public CasePriority Priority { get; set; }
        public LetterType Type { get; set; }
        public DateTime? ClosedDate { get; set; }
        public string? Comments { get; set; }
        public DateTime? AssignedOn { get; set; }

        #region Navigation Properties
        public virtual Tender Tender { get; set; } = null!;

        public virtual Agent? Agent { get; set; }

        public virtual ICollection<CaseLetter> CaseLetters { get; set; }
            = new List<CaseLetter>();

        public virtual ICollection<AuditLog> AuditLogs { get; set; }
            = new List<AuditLog>();

        public virtual ICollection<CaseNote> CaseNotes { get; set; }
            = new List<CaseNote>();

        public virtual ICollection<ComplianceFinding> ComplianceFindings { get; set; }
            = new List<ComplianceFinding>();

        public virtual ICollection<ComplianceAction> ComplianceActions { get; set; }
            = new List<ComplianceAction>();

        #endregion
    }
}
