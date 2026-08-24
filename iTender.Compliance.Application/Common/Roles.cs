namespace iTender.Compliance.Application.Common
{
    public static class Roles
    {
        public const string Director = "Director";

        public const string ComplianceManager = "Compliance Manager";

        public const string ComplianceOfficer = "Compliance Officer";

        public const string ComplianceAdministrator = "Compliance Administrator";

        public static readonly string[] All =
    [
        Director,
        ComplianceManager,
        ComplianceOfficer,
        ComplianceAdministrator
    ];
    }
}
