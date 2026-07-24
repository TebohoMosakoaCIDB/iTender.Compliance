namespace iTender.Compliance.Domain.Entities
{
    public class CaseNote : BaseEntity
    {
        public Guid ComplianceCaseId { get; set; }

        public string Comment { get; set; } = string.Empty;

        public Guid? CreatedByUserId { get; set; }

        public virtual ComplianceCase ComplianceCase { get; set; } = null!;
    }
}
