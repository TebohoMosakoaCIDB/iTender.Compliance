using iTender.Compliance.Application.Models.Reports;

namespace iTender.Compliance.Application.Models.TenderSync
{
    public class AdministrationModel
    {
        public SyncReportModel Statistics { get; set; } = new();

        public List<iTender.Compliance.Domain.Entities.TenderSync> Synchronizations { get; set; } = [];

        public DateTime? LastSynchronization { get; set; }

        public DateTime? NextSynchronization { get; set; }

        public bool IsSynchronizationRunning { get; set; }
    }
}
