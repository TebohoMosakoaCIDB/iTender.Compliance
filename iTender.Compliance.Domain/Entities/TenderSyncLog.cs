using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Domain.Entities
{
    public class TenderSyncLog : BaseEntity
    {
        public Guid TenderSyncId { get; set; }
        public SyncLogType Type { get; set; }
        public SyncLogLevel Level { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? TenderNumber { get; set; }
        public virtual TenderSync TenderSync { get; set; } = null!;
    }
}
