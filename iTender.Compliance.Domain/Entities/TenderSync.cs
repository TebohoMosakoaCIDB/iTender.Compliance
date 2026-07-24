using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Domain.Entities
{
    public class TenderSync : BaseEntity
    {
        public DateTime StartedOn { get; set; }
        public DateTime? CompletedOn { get; set; }
        public SyncStatus Status { get; set; }
        public bool IsManual { get; set; }
        public Guid? StartedByUserId { get; set; }
        public int TotalRetrieved { get; set; }
        public int TotalCompliant { get; set; }
        public int TotalNonCompliant { get; set; }
        public int CasesCreated { get; set; }
        public int ErrorCount { get; set; }
        public string? Notes { get; set; }
        public virtual ICollection<Tender> Tenders { get; set; }
    = new List<Tender>();

        public virtual ICollection<TenderSyncLog> Logs { get; set; }
    = new List<TenderSyncLog>();
    }
}
