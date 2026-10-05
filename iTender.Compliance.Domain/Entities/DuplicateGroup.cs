// iTender.Compliance.Domain/Entities/DuplicateGroup.cs
using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Domain.Entities
{
    public class DuplicateGroup : BaseEntity
    {
        public Guid TenderSyncId { get; set; }
        public DuplicateStatus Status { get; set; }

        public Guid? ConfirmedByUserId { get; set; }
        public DateTime? ConfirmedOn { get; set; }
        public string? CoComment { get; set; }

        #region Navigation Properties

        public virtual TenderSync TenderSync { get; set; } = null!;

        public virtual ICollection<DuplicateCandidate> Candidates { get; set; }
            = new List<DuplicateCandidate>();

        #endregion
    }
}