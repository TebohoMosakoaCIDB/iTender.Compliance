using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Application.DTOs.Reports
{
    public class SynchronizationReportModel
    {
        public DateTime StartedOn { get; set; }

        public DateTime? CompletedOn { get; set; }

        public bool IsManual { get; set; }

        public SyncStatus Status { get; set; }

        public int TotalRetrieved { get; set; }

        public int CasesCreated { get; set; }

        public int TotalCompliant { get; set; }

        public int TotalNonCompliant { get; set; }

        public int ErrorCount { get; set; }
    }
}