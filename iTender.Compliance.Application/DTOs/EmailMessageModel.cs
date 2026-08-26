namespace iTender.Compliance.Application.DTOs
{
    public class EmailMessageModel
    {
        public string ToAddress { get; set; } = string.Empty;

        public string? ToName { get; set; }

        public string Subject { get; set; } = string.Empty;

        /// <summary>HTML body.</summary>
        public string Body { get; set; } = string.Empty;

        public List<string> AttachmentPaths { get; set; } = new();
    }
}