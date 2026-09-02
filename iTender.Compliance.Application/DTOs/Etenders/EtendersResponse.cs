namespace iTender.Compliance.Application.DTOs.Etenders
{
    public class EtendersResponse
    {
        public string? Uri { get; set; }

        public string? Version { get; set; }

        public DateTime? PublishedDate { get; set; }

        public EtendersPublisher? Publisher { get; set; }

        public string? License { get; set; }

        public string? PublicationPolicy { get; set; }

        public List<EtendersRelease> Releases { get; set; } = [];

        public EtendersLinks? Links { get; set; }
    }

    public class EtendersPublisher
    {
        public string? Name { get; set; }

        public string? Uri { get; set; }
    }

    public class EtendersLinks
    {
        public string? Next { get; set; }
    }
}
