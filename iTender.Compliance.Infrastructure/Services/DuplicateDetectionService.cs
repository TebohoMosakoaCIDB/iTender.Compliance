using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Application.Interfaces.Services;
using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Infrastructure.Services
{
    public class DuplicateDetectionService : IDuplicateDetectionService
    {
        private readonly ITenderRepository _tenderRepository;

        public DuplicateDetectionService(ITenderRepository tenderRepository)
        {
            _tenderRepository = tenderRepository;
        }

        public async Task<IReadOnlyList<DuplicateCandidate>> FindPotentialDuplicatesAsync(
            Tender tender,
            CancellationToken cancellationToken = default)
        {
            var candidates = new List<DuplicateCandidate>();

            // Rule 1 - Normalized title
            if (!string.IsNullOrWhiteSpace(tender.NormalizedTitle))
            {
                var titleMatches = await _tenderRepository
                    .GetByNormalizedTitleAsync(tender.NormalizedTitle, tender.Id, cancellationToken);

                foreach (var match in titleMatches)
                {
                    candidates.Add(new DuplicateCandidate
                    {
                        Id = Guid.NewGuid(),
                        TenderId = match.Id,
                        MatchType = DuplicateMatchType.NormalizedTitle,
                        ConfidenceScore = 0.85,
                        MatchedOn = tender.NormalizedTitle
                    });
                }
            }

            // Rule 2 - Same employer + same closing date
            if (!string.IsNullOrWhiteSpace(tender.EmployerName))
            {
                var employerMatches = await _tenderRepository
                    .GetByEmployerAndClosingDateAsync(
                        tender.EmployerName,
                        tender.ClosingDate,
                        tender.Id,
                        cancellationToken);

                foreach (var match in employerMatches)
                {
                    if (candidates.Any(c => c.TenderId == match.Id))
                        continue;

                    candidates.Add(new DuplicateCandidate
                    {
                        Id = Guid.NewGuid(),
                        TenderId = match.Id,
                        MatchType = DuplicateMatchType.EmployerAndClosingDate,
                        ConfidenceScore = 0.70,
                        MatchedOn = $"{tender.EmployerName}|{tender.ClosingDate:yyyy-MM-dd}"
                    });
                }
            }

            return candidates;
        }
    }
}
