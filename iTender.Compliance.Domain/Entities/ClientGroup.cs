using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Domain.Entities
{
    public class ClientGroup : BaseEntity
    {
        /// <summary>The canonical (normalised) employer name this group represents.</summary>
        public string NormalizedEmployerName { get; set; } = string.Empty;

        /// <summary>The display employer name as captured on the tender.</summary>
        public string EmployerName { get; set; } = string.Empty;

        public Guid? AgentId { get; set; }

        public ClientGroupStatus Status { get; set; }

        /// <summary>Primary recipient for the consolidated letter.</summary>
        public string? ContactName { get; set; }
        public string? ContactEmail { get; set; }

        /// <summary>Semicolon-separated CC addresses.</summary>
        public string? CcEmails { get; set; }

        /// <summary>The CaseLetter generated for this group.</summary>
        public Guid? ConsolidatedLetterId { get; set; }

        public DateTime? IssuedOn { get; set; }

        public string? AnnexureFileName { get; set; }
        public string? AnnexureFilePath { get; set; }

        #region Navigation

        public virtual Agent? Agent { get; set; }

        public virtual ICollection<ClientGroupCase> Cases { get; set; }
            = new List<ClientGroupCase>();

        public virtual CaseLetter? ConsolidatedLetter { get; set; }

        #endregion
    }
}