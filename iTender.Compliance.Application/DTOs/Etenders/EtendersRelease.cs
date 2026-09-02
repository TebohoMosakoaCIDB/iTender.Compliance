namespace iTender.Compliance.Application.DTOs.Etenders
{
    public class EtendersRelease
    {
        public string? Ocid { get; set; }

        public string? Id { get; set; }

        public DateTime? Date { get; set; }

        public List<string> Tag { get; set; } = [];

        public string? InitiationType { get; set; }

        public EtendersTender? Tender { get; set; }

        public EtendersPlanning? Planning { get; set; }

        public List<object> Parties { get; set; } = [];

        public EtendersEntity? Buyer { get; set; }

        public string? Language { get; set; }

        public List<object> Awards { get; set; } = [];

        public List<object> Contracts { get; set; } = [];
    }

    public class EtendersPlanning
    {
        public string? Rationale { get; set; }

        public EtendersBudget? Budget { get; set; }

        public List<object> Documents { get; set; } = [];
    }

    public class EtendersBudget
    {
        public string? Description { get; set; }
    }
}
