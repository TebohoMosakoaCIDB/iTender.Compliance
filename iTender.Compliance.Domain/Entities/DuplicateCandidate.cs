// iTender.Compliance.Domain/Entities/DuplicateCandidate.cs
using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Domain.Entities
{
    public class DuplicateCandidate : BaseEntity
    {
        public Guid DuplicateGroupId { get; set; }
        public Guid TenderId { get; set; }
        public DuplicateMatchType MatchType { get; set; }
        public double ConfidenceScore { get; set; }
        public string? MatchedOn { get; set; }

        #region Navigation Properties

        public virtual DuplicateGroup DuplicateGroup { get; set; } = null!;

        public virtual Tender Tender { get; set; } = null!;

        #endregion
    }
}