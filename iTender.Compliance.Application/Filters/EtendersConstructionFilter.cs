using iTender.Compliance.Application.DTOs.Etenders;

namespace iTender.Compliance.Application.Filters
{
    public class EtendersConstructionFilter
    {
        private static readonly HashSet<string> AllowedCategories =
            new(StringComparer.OrdinalIgnoreCase)
            {
            "Architectural and engineering activities; technical testing and analysis",
            "Civil engineering",
            "Construction",
            "Construction of buildings",
            "Services: Building",
            "Services: Electrical",
            "Services: Civil",
            "Specialised construction activities",
            "Water supply; sewerage, waste management and remediation activities"
            };

        public bool IsConstruction(EtendersTender? tender)
        {
            if (tender == null)
                return false;

            if (string.IsNullOrWhiteSpace(tender.Category))
                return false;

            return AllowedCategories.Contains(
                tender.Category.Trim());
        }

        public IReadOnlyList<EtendersRelease> Filter(
            IEnumerable<EtendersRelease> releases)
        {
            return releases
                .Where(release => IsConstruction(release.Tender))
                .ToList();
        }
    }
}
