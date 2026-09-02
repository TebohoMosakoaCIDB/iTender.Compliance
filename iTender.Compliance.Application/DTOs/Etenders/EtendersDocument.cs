namespace iTender.Compliance.Application.DTOs.Etenders
{
    public class EtendersDocument
    {
        public string? Id { get; set; }

        public string? DocumentType { get; set; }

        public string? Title { get; set; }

        public string? Description { get; set; }

        public string? Url { get; set; }

        public DateTime? DatePublished { get; set; }

        public DateTime? DateModified { get; set; }

        public string? Format { get; set; }

        public string? Language { get; set; }
    }
}
