namespace iTender.Compliance.Domain.Entities
{
    public class ClientResponseDocument : BaseEntity
    {
        public Guid ClientResponseId { get; set; }

        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// Secure storage reference or relative storage path.
        /// Do not expose this directly to unauthorised users.
        /// </summary>
        public string StoragePath { get; set; } = string.Empty;

        public string ContentType { get; set; } = string.Empty;

        public long FileSizeBytes { get; set; }

        public DateTime UploadedOn { get; set; }

        public Guid? UploadedBy { get; set; }

        public virtual ClientResponse ClientResponse { get; set; } = null!;
    }
}