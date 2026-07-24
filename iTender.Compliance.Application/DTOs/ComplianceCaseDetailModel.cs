namespace iTender.Compliance.Application.DTOs
{
    public class ComplianceCaseDetailModel
    {
        public Guid Id { get; set; }

        public TenderDetailModel Tender { get; set; } = new();

        public CaseDetailModel Case { get; set; } = new();

        public List<CaseLetterModel> Letters { get; set; } = [];

        public List<AuditLogModel> Timeline { get; set; } = [];
        public List<CaseNoteModel> Notes { get; set; } = [];
    }
}
