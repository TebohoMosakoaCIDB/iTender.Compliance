using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Application.DTOs
{
    public class ClientGroupSearchModel
    {
        public string? SearchText { get; set; }
        public ClientGroupStatus? Status { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public Guid? AgentId { get; set; }
    }
}
