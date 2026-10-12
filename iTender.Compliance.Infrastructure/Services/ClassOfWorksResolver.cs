using iTender.Compliance.Application.DTOs;
using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Application.Interfaces.Services;
using iTender.Compliance.Domain.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace iTender.Compliance.Infrastructure.Services
{
    public class ClassOfWorksResolver : IClassOfWorksResolver
    {
        private readonly IClassOfWorkTypeRepository _classOfWorkTypeRepository;
        private readonly ILogger<ClassOfWorksResolver> _logger;

        private IReadOnlyList<ClassOfWorkTypeModel>? _cachedTypes;

        public ClassOfWorksResolver(
            IClassOfWorkTypeRepository classOfWorkTypeRepository,
            ILogger<ClassOfWorksResolver> logger)
        {
            _classOfWorkTypeRepository = classOfWorkTypeRepository;
            _logger = logger;
        }

        // =============================================================
        // 1) Resolve a class from the advert text (existing behaviour).
        //    Priority: grade code > CRM description > keyword contingency.
        // =============================================================
        public async Task<string?> ResolveAsync(
            Tender tender,
            CancellationToken ct = default)
        {
            var haystack = BuildHaystack(tender);

            if (string.IsNullOrWhiteSpace(haystack))
                return null;

            _cachedTypes ??= await _classOfWorkTypeRepository.GetAllAsync(ct);

            if (_cachedTypes.Count == 0)
            {
                _logger.LogWarning(
                    "No class-of-works types from CRM - cannot resolve for {TenderNumber}.",
                    tender.TenderNumber);
                return null;
            }

            var validCodes = _cachedTypes
                .Where(c => !string.IsNullOrWhiteSpace(c.Name))
                .Select(c => c.Name.Trim().ToUpperInvariant())
                .ToHashSet();

            // 1a) Strongest signal: CIDB grade+code, e.g. "1SN", "3GB", "5CE"
            var gradeCodeMatches = Regex
                .Matches(haystack, @"\b\d{1,2}\s?([A-Z]{2})\b", RegexOptions.IgnoreCase)
                .Select(m => m.Groups[1].Value.ToUpperInvariant())
                .Where(validCodes.Contains)
                .Distinct()
                .ToList();

            if (gradeCodeMatches.Count == 1)
            {
                _logger.LogInformation(
                    "Resolved class '{Code}' for {TenderNumber} from CIDB grade pattern.",
                    gradeCodeMatches[0], tender.TenderNumber);
                return gradeCodeMatches[0];
            }

            if (gradeCodeMatches.Count > 1)
            {
                _logger.LogWarning(
                    "Multiple CIDB codes found in {TenderNumber}: {Codes}",
                    tender.TenderNumber, string.Join(",", gradeCodeMatches));
            }

            // 1b) CRM Description substring match
            var byDescription = _cachedTypes
                .Where(c => !string.IsNullOrWhiteSpace(c.Description))
                .OrderByDescending(c => c.Description!.Length)
                .FirstOrDefault(c => haystack.Contains(c.Description!, StringComparison.OrdinalIgnoreCase));

            if (byDescription != null)
            {
                _logger.LogInformation(
                    "Resolved class '{Code}' for {TenderNumber} from CRM description match.",
                    byDescription.Name, tender.TenderNumber);
                return byDescription.Name;
            }

            // 1c) Keyword contingency
            foreach (var kvp in CategoryMappingService.ClassKeywords)
            {
                if (!validCodes.Contains(kvp.Key))
                    continue;

                if (kvp.Value.Any(k => ContainsKeyword(haystack, k)))
                {
                    _logger.LogInformation(
                        "Resolved class '{Code}' for {TenderNumber} via keyword contingency.",
                        kvp.Key, tender.TenderNumber);
                    return kvp.Key;
                }
            }

            _logger.LogWarning(
                "Could not resolve class of works for {TenderNumber}.",
                tender.TenderNumber);
            return null;
        }

        // =============================================================
        // 2) Verify the advert text doesn't CONTRADICT a class claimed by category.
        //
        //    Note the semantics:
        //    - If the text contains a keyword for the claimed class  -> supported.
        //    - If the text contains a keyword for a DIFFERENT class  -> contradicted (return false).
        //    - If the text is silent about class entirely            -> supported (trust the category).
        //
        //    This matches how a Compliance Officer reads an advert: "if the advert
        //    says nothing definitive, go with the category; if it contradicts, flag it."
        // =============================================================
        public Task<bool> DescriptionSupportsClassAsync(
            Tender tender,
            string classOfWorks,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(classOfWorks))
                return Task.FromResult(false);

            var haystack = BuildHaystack(tender);

            if (string.IsNullOrWhiteSpace(haystack))
                return Task.FromResult(false);

            if (!CategoryMappingService.ClassKeywords.TryGetValue(classOfWorks, out var ownKeywords))
            {
                // Unknown class -> don't block. The class validator will handle it downstream.
                _logger.LogInformation(
                    "No keyword list for class '{Class}' - skipping description verification for {TenderNumber}.",
                    classOfWorks, tender.TenderNumber);
                return Task.FromResult(true);
            }

            // 1) Does the text support the claimed class?
            var ownMatch = ownKeywords.FirstOrDefault(k => ContainsKeyword(haystack, k));

            if (ownMatch != null)
            {
                _logger.LogInformation(
                    "Description supports class '{Class}' for {TenderNumber} (own keyword '{Keyword}').",
                    classOfWorks, tender.TenderNumber, ownMatch);
                return Task.FromResult(true);
            }

            // 2) Does the text contradict it by pointing at a DIFFERENT class?
            var contradictingKeyword = CategoryMappingService.ClassKeywords
                .Where(kv => !string.Equals(kv.Key, classOfWorks, StringComparison.OrdinalIgnoreCase))
                .SelectMany(kv => kv.Value.Select(k => new { OtherClass = kv.Key, Keyword = k }))
                .FirstOrDefault(x => ContainsKeyword(haystack, x.Keyword));

            if (contradictingKeyword != null)
            {
                _logger.LogInformation(
                    "Description contradicts class '{Class}' for {TenderNumber} - " +
                    "matched '{Keyword}' from class '{OtherClass}'.",
                    classOfWorks, tender.TenderNumber,
                    contradictingKeyword.Keyword, contradictingKeyword.OtherClass);

                return Task.FromResult(false);
            }

            // 3) Text is silent about class -> trust the category.
            _logger.LogInformation(
                "Description is silent about class for {TenderNumber} - " +
                "trusting category-claimed class '{Class}'.",
                tender.TenderNumber, classOfWorks);

            return Task.FromResult(true);
        }
        // =============================================================
        // Helpers
        // =============================================================
        private static string BuildHaystack(Tender tender)
        {
            return string.Join(" ", new[]
            {
                tender.Title,
                tender.Description,
                tender.SpecialConditions
            }
            .Where(x => !string.IsNullOrWhiteSpace(x)));
        }

        private static bool ContainsKeyword(string haystack, string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return false;

            // Multi-word phrases: plain Contains is fine.
            if (keyword.Contains(' '))
                return haystack.Contains(keyword, StringComparison.OrdinalIgnoreCase);

            // Single word: require word boundaries so "road" doesn't match "broad".
            return Regex.IsMatch(
                haystack,
                $@"\b{Regex.Escape(keyword)}\b",
                RegexOptions.IgnoreCase);
        }
    }
}