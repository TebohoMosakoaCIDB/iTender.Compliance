namespace iTender.Compliance.Application.DTOs
{
    public class AgentLookupModel
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public string FullName { get; set; } = string.Empty;

        public int OpenCases { get; set; }
    }
}
