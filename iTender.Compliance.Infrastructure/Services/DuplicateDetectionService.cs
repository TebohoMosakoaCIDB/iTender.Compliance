using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Application.Interfaces.Services;
using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Domain.Enums;
using iTender.Compliance.Infrastructure.Data;
using iTender.Compliance.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace iTender.Compliance.Infrastructure.Services
{
    public class DuplicateDetectionService : RepositoryBase, IDuplicateDetectionService
    {
        private readonly ITenderRepository _tenderRepository;

        public DuplicateDetectionService(
            IDbContextFactory<ComplianceDbContext> contextFactory,
            ITenderRepository tenderRepository)
            : base(contextFactory)
        {
            _tenderRepository = tenderRepository;
        }

        public async Task<IReadOnlyList<DuplicateCandidate>> FindPotentialDuplicatesAsync(
            Tender tender, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(tender.NormalizedTitle)
                || string.IsNullOrWhiteSpace(tender.NormalizedEmployerName))
            {
                return Array.Empty<DuplicateCandidate>();
            }

            await using var context = await CreateContextAsync();

            var candidates = await context.Tenders
                .AsNoTracking()
                .Where(t => t.Id != tender.Id)
                .Where(t => t.NormalizedTitle == tender.NormalizedTitle)
                .Where(t => t.NormalizedEmployerName == tender.NormalizedEmployerName)
                .Select(t => new DuplicateCandidate
                {
                    Id = Guid.NewGuid(),
                    TenderId = t.Id,
                    MatchType = DuplicateMatchType.NormalizedTitle,
                    ConfidenceScore = 0.95,
                    MatchedOn = "NormalizedEmployerName+NormalizedTitle"
                })
                .ToListAsync(ct);

            return candidates;
        }
    }
}