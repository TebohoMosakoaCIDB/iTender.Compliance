namespace iTender.Compliance.Application.DTOs.Etenders
{
    public class EtendersTender
    {
        public string? Id { get; set; }

        public string? Title { get; set; }

        public string? Status { get; set; }

        public string? Category { get; set; }

        public string? Province { get; set; }

        public string? DeliveryLocation { get; set; }

        public string? SpecialConditions { get; set; }

        public string? MainProcurementCategory { get; set; }

        public List<string> AdditionalProcurementCategories { get; set; } = [];

        public string? Description { get; set; }

        public EtendersValue? Value { get; set; }

        public List<EtendersDocument> Documents { get; set; } = [];

        public EtendersTenderPeriod? TenderPeriod { get; set; }

        public List<object> Tenderers { get; set; } = [];

        public EtendersEntity? ProcuringEntity { get; set; }

        public string? ProcurementMethod { get; set; }

        public string? ProcurementMethodDetails { get; set; }

        public EtendersBriefingSession? BriefingSession { get; set; }

        public EtendersContactPerson? ContactPerson { get; set; }
    }
}
