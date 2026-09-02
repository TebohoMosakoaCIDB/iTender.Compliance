namespace iTender.Compliance.Application.DTOs.Etenders
{
    public class EtendersBriefingSession
    {
        public bool IsSession { get; set; }

        public bool Compulsory { get; set; }

        public DateTime? Date { get; set; }

        public string? Venue { get; set; }
    }
}
