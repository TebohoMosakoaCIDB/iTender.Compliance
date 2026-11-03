using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Application.DTOs
{
    public class NonCompliantCaseReportFilter
    {
        public Guid? AgentId { get; set; }
        public string? EmployerName { get; set; }
        public string? Province { get; set; }
        public CaseStatus? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
