namespace iTender.Compliance.Infrastructure.CRM
{
    public class CrmOptions
    {
        public const string SectionName = "Crm";
        public required string CrmURL { get; set; }
        public required string ClientId { get; set; }
        public required string ClientSecret { get; set; }
    }
}
