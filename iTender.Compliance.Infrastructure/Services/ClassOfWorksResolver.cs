using iTender.Compliance.Application.DTOs;
using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Application.Interfaces.Services;
using iTender.Compliance.Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

// NOTE: add whichever `using` actually resolves ClassOfWorkTypeModel in your project -
// I only have the class body you pasted, not its file header, so I can't be sure if it's
// Domain.Entities (as used here), Application.DTOs, or something CRM-specific. If this
// doesn't resolve, that's the fix - the rest of the file doesn't need to change.

namespace iTender.Compliance.Infrastructure.Services
{
    public class ClassOfWorksResolver : IClassOfWorksResolver
    {
        private readonly IClassOfWorkTypeRepository _classOfWorkTypeRepository;
            private readonly ILogger<ClassOfWorksResolver> _logger;

        // Cached for the lifetime of this instance (one DI scope = one sync run, so this is
        // effectively "once per sync batch" rather than once per tender) instead of hitting
        // the CRM on every single tender processed in the batch.
        private IReadOnlyList<ClassOfWorkTypeModel>? _cachedTypes;

        public ClassOfWorksResolver(
            IClassOfWorkTypeRepository classOfWorkTypeRepository,
            ILogger<ClassOfWorksResolver> logger)
        {
            _classOfWorkTypeRepository = classOfWorkTypeRepository;
            _logger = logger;
        }

        public async Task<string?> ResolveAsync(Tender tender, CancellationToken ct = default)
        {
            var haystack = string.Join(" ", new[] { tender.Title, tender.Description, tender.SpecialConditions }
                .Where(x => !string.IsNullOrWhiteSpace(x)));

            if (string.IsNullOrWhiteSpace(haystack)) return null;

            _cachedTypes ??= await _classOfWorkTypeRepository.GetAllAsync(ct);

            if (_cachedTypes.Count == 0)
            {
                _logger.LogWarning("No class-of-works types from CRM - cannot resolve for {TenderNumber}.", tender.TenderNumber);
                return null;
            }

            var validCodes = _cachedTypes
                .Where(c => !string.IsNullOrWhiteSpace(c.Name))
                .Select(c => c.Name.Trim().ToUpperInvariant())
                .ToHashSet();

            // 1) Strongest: CIDB grade+code, e.g. "1SN", "3GB", "5CE"
            var gradeCodeMatches = Regex
                .Matches(haystack, @"\b\d{1,2}\s?([A-Z]{2})\b", RegexOptions.IgnoreCase)
                .Select(m => m.Groups[1].Value.ToUpperInvariant())
                .Where(validCodes.Contains)
                .Distinct()
                .ToList();

            if (gradeCodeMatches.Count == 1)
            {
                _logger.LogInformation("Resolved class '{Code}' for {TenderNumber} from CIDB grade pattern.",
                    gradeCodeMatches[0], tender.TenderNumber);
                return gradeCodeMatches[0];
            }

            if (gradeCodeMatches.Count > 1)
            {
                _logger.LogWarning("Multiple CIDB codes found in {TenderNumber}: {Codes}",
                    tender.TenderNumber, string.Join(",", gradeCodeMatches));
                // fall through
            }

            // 2) Description match (CRM Description text)
            var byDescription = _cachedTypes
                .Where(c => !string.IsNullOrWhiteSpace(c.Description))
                .OrderByDescending(c => c.Description.Length)
                .FirstOrDefault(c => haystack.Contains(c.Description, StringComparison.OrdinalIgnoreCase));

            if (byDescription != null)
            {
                _logger.LogInformation("Resolved class '{Code}' for {TenderNumber} from description match.",
                    byDescription.Name, tender.TenderNumber);
                return byDescription.Name;
            }

            // 3) Contingency: keyword rules
            foreach (var rule in KeywordRules)
            {
                if (rule.Keywords.Any(k => ContainsKeyword(haystack, k)))
                {
                    if (!validCodes.Contains(rule.Code))
                        continue;

                    _logger.LogInformation(
                        "Resolved class '{Code}' for {TenderNumber} via keyword contingency.",
                        rule.Code, tender.TenderNumber);
                    return rule.Code;
                }
            }

            _logger.LogWarning("Could not resolve class of works for {TenderNumber}.", tender.TenderNumber);
            return null;
        }

        private static bool ContainsKeyword(string haystack, string keyword)
        {
            if (keyword.Contains(' '))
                return haystack.Contains(keyword, StringComparison.OrdinalIgnoreCase);

            return Regex.IsMatch(
                haystack,
                $@"\b{Regex.Escape(keyword)}\b",
                RegexOptions.IgnoreCase);
        }

        private static readonly (string[] Keywords, string Code)[] KeywordRules =
        {
            (new[] { "access road", "road", "street", "gravel road", "paved road",
                     "asphalt", "reseal", "pothole", "culvert", "stormwater",
                     "bridge", "river crossing" }, "CE"),

            (new[] { "water reticulation", "water pipeline", "sewer", "sewerage",
                     "pump station", "reservoir", "dam", "borehole",
                     "water treatment", "wastewater" }, "CE"),

            (new[] { "building", "school", "clinic", "hospital", "office block",
                     "library", "hall", "toilet", "ablution", "classroom",
                     "crèche", "creche", "housing", "house" }, "GB"),

            (new[] { "substation", "overhead line", "ohtl", "electrification",
                     "streetlight", "high mast", "transformer", "switchgear" }, "EB"),

            (new[] { "pump", "cooling tower", "boiler", "mechanical", "hvac",
                     "chiller", "conveyor", "turbine" }, "ME"),

            (new[] { "roof waterproofing", "waterproofing", "damp proofing",
                     "roof leak" }, "SN"),

            (new[] { "piling", "pile foundation", "shaft sinking" }, "SJ"),

            (new[] { "demolition", "blasting" }, "SE"),

            (new[] { "fire prevention", "fire protection", "sprinkler system",
                     "fire detection" }, "SF"),

            (new[] { "landscaping", "horticulture", "gardening" }, "SH"),

            (new[] { "lift", "elevator", "escalator" }, "SI"),

            (new[] { "road marking", "road sign", "signage" }, "SK"),

            (new[] { "structural steel", "steelwork", "steel structure" }, "SL"),
        };
    }
}