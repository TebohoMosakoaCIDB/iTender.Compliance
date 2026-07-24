namespace iTender.Compliance.Application.DTOs
{
    public class DocumentResult
    {
        public string FileName { get; set; } = string.Empty;

        public byte[] Content { get; set; } = [];

        public string? FilePath { get; set; }
    }
}
