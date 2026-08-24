using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Domain.Entities
{
    public class CorrespondenceTemplateModel : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public CorrespondenceTemplateType Type { get; set; }

        public CorrespondenceTemplateStatus Status { get; set; }

        public int Version { get; set; } = 1;

        public string Subject { get; set; } = string.Empty;

        public string Body { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime? UpdatedOn { get; set; }

        public string? CreatedBy { get; set; }

        public string? UpdatedBy { get; set; }

        public DateTime? ApprovedOn { get; set; }

        public string? ApprovedBy { get; set; }

        public string? ApprovalComments { get; set; }
    }
}
