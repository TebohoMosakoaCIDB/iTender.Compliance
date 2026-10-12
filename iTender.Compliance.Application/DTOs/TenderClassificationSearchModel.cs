namespace iTender.Compliance.Application.DTOs
{
    public class TenderClassificationSearchModel
    {
        public string? SearchText { get; set; }
        public bool? IsConstruction { get; set; }
        public bool? ReviewedOnly { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public Guid? AgentId { get; set; }
    }
}
