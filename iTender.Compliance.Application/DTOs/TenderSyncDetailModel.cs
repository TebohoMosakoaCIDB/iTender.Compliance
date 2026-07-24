namespace iTender.Compliance.Application.DTOs
{
    public class TenderSyncDetailModel
    {
        public Guid Id { get; set; }

        public DateTime StartedOn { get; set; }

        public DateTime? CompletedOn { get; set; }

        public bool IsManual { get; set; }

        public int TotalRetrieved { get; set; }

        public int TotalCompliant { get; set; }

        public int TotalNonCompliant { get; set; }

        public int CasesCreated { get; set; }

        public int ErrorCount { get; set; }

        public List<TenderSyncLogModel> Logs { get; set; } = [];
    }
}
