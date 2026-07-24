namespace iTender.Compliance.Application.Models.Reports
{
    public class AuditReportModel
    {
        public DateTime Date { get; set; }

        public string User { get; set; } = string.Empty;

        public string Action { get; set; } = string.Empty;

        public string Entity { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
    }
}
