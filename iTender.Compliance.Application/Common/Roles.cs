namespace iTender.Compliance.Application.Common
{
    public static class Roles
    {
        public const string Administrator = "Administrator";

        public const string Supervisor = "Supervisor";

        public const string ComplianceAgent = "Compliance Agent";

        public static readonly IReadOnlyList<string> All =
        [
            Administrator,
            Supervisor,
            ComplianceAgent
        ];
    }
}
