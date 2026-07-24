namespace iTender.Compliance.Domain.Entities
{
    public abstract class BaseEntity
    {
        public Guid Id { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        public Guid? CreatedBy { get; set; }

        public DateTime? ModifiedOn { get; set; } = DateTime.UtcNow;

        public Guid? ModifiedBy { get; set; }
    }
}
