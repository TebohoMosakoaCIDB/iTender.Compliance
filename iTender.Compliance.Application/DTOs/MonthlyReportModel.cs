namespace iTender.Compliance.Application.DTOs
{
    public class MonthlyReportModel
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string PeriodLabel { get; set; } = string.Empty;

        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }

        // Core counts from BR-9.4
        public int ProjectsIdentified { get; set; }
        public int ProjectsVerified { get; set; }
        public int ProjectsRegistered { get; set; }
        public int ProjectsNotRegistered { get; set; }
        public int InitiallyNonCompliant { get; set; }
        public int Resolved { get; set; }
        public int ResolvedAfterIL { get; set; }
        public int ResolvedAfterCN { get; set; }
        public int RemainingNonCompliant { get; set; }
        public int ReferredToEnforcement { get; set; }
        public int EscalatedToAgsa { get; set; }
        public int CorrectiveActionsTaken { get; set; }

        // SLA
        public int CasesOpened { get; set; }
        public int CasesClosed { get; set; }
        public decimal AverageCaseAgeDays { get; set; }
        public int CasesPastSlaDeadline { get; set; }
    }
}