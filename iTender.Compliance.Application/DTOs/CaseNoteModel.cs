namespace iTender.Compliance.Application.DTOs
{
    public class CaseNoteModel
    {
        public Guid Id { get; set; }

        public string Note { get; set; } = string.Empty;

        public DateTime CreatedOn { get; set; }

        public string? CreatedByName { get; set; }
    }
}
