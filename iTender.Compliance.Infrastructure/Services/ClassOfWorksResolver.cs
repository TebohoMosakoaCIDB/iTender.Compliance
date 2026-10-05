using iTender.Compliance.Application.DTOs;
using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Application.Interfaces.Services;
using iTender.Compliance.Domain.Entities;
using Microsoft.Extensions.Logging;

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

        public async Task<string?> ResolveAsync(
            Tender tender,
            CancellationToken cancellationToken = default)
        {
            // OCDS only ever carries a generic procurement category (works/goods/services) -
            // already used upstream to filter to construction tenders in the first place - never
            // a CIDB-specific class-of-works code. The only place a real class name might show up
            // is free text on the advert itself, so search that against CIDB's actual class list
            // from the CRM rather than relying on a source field that was never going to be there.
            var haystack = string.Join(" ", new[]
                {
                    tender.Title,
                    tender.Description,
                    tender.SpecialConditions
                }
                .Where(x => !string.IsNullOrWhiteSpace(x)));

            if (string.IsNullOrWhiteSpace(haystack))
                return null;

            _cachedTypes ??= await _classOfWorkTypeRepository.GetAllAsync(cancellationToken);

            if (_cachedTypes.Count == 0)
            {
                _logger.LogWarning(
                    "No class-of-works types returned from the CRM - cannot resolve a class for tender {TenderNumber}.",
                    tender.TenderNumber);

                return null;
            }

            // Longest name first, so a more specific class ("General Building") wins over a
            // shorter one that might otherwise coincidentally be a substring match.
            var match = _cachedTypes
                .Where(c => !string.IsNullOrWhiteSpace(c.Name))
                .OrderByDescending(c => c.Name.Length)
                .FirstOrDefault(c => haystack.Contains(c.Name, StringComparison.OrdinalIgnoreCase));

            if (match != null)
            {
                _logger.LogInformation(
                    "Resolved class of works '{ClassName}' for tender {TenderNumber} from advert text.",
                    match.Name,
                    tender.TenderNumber);
            }

            return match?.Name;
        }
    }
}