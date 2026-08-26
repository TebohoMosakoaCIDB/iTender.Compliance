using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Domain.Entities
{
    public class Agent : BaseEntity
    {
        public Guid UserId { get; set; }
        public string EmployeeNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        /// <summary>Regulatory Compliance Manager - can approve letters, review objections and extensions.</summary>
        public bool IsManager { get; set; }

        public bool AutoAssignEnabled { get; set; } = true;
        public int MaximumOpenCases { get; set; }
        public int DisplayOrder { get; set; }
        public string? Notes { get; set; }
        public AgentLevel Level { get; set; }
        public string? JobTitle { get; set; }
        public string? HeaderImagePath { get; set; }
        public string? SignatureImagePath { get; set; }
        public string? FooterText { get; set; }

        #region Navigation Properties

        public virtual ICollection<ComplianceCase> ComplianceCases { get; set; }
            = new List<ComplianceCase>();
        #endregion
    }
}
