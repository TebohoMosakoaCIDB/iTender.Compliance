namespace iTender.Compliance.Application.Models.Reports
{
    public class SyncReportModel
    {
        public int TotalSynchronizations { get; set; }

        public int TotalRetrieved { get; set; }

        public int TotalCasesCreated { get; set; }

        public int FailedSynchronizations { get; set; }
    }
}
