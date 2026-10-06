using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Application.DTOs
{
    public class ClientGroupSummaryModel
    {
        public Guid Id { get; set; }
        public string EmployerName { get; set; } = string.Empty;
        public ClientGroupStatus Status { get; set; }
        public int TenderCount { get; set; }
        public string? AgentName { get; set; }
    }
}
