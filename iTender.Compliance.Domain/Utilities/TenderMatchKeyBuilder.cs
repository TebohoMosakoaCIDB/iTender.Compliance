// iTender.Compliance.Domain/Utilities/TenderMatchKeyBuilder.cs
using iTender.Compliance.Domain.Utilities; // for EmployerNameNormalizer / TenderTitleNormalizer

namespace iTender.Compliance.Domain.Utilities
{
    public static class TenderMatchKeyBuilder
    {
        public static string Build(string? employer, string? title)
        {
            var e = EmployerNameNormalizer.Normalize(employer) ?? "";
            var t = TenderTitleNormalizer.Normalize(title) ?? "";
            return $"{e}|{t}";
        }
    }
}