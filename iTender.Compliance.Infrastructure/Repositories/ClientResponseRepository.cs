using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Domain.Enums;
using iTender.Compliance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace iTender.Compliance.Infrastructure.Repositories
{
    public class ClientResponseRepository
        : RepositoryBase, IClientResponseRepository
    {
        public ClientResponseRepository(
            IDbContextFactory<ComplianceDbContext> contextFactory)
            : base(contextFactory)
        {
        }

        public async Task<ClientResponse?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            return await context.ClientResponses
                .Include(x => x.Documents)
                .FirstOrDefaultAsync(
                    x => x.Id == id,
                    cancellationToken);
        }

        public async Task<IReadOnlyList<ClientResponse>> GetByCaseIdAsync(
            Guid complianceCaseId,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            return await context.ClientResponses
                .Include(x => x.Documents)
                .Where(x => x.ComplianceCaseId == complianceCaseId)
                .OrderByDescending(x => x.SubmittedOn)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(
            ClientResponse response,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            await context.ClientResponses.AddAsync(
                response,
                cancellationToken);

            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(
            ClientResponse response,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            context.ClientResponses.Update(response);

            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<bool> HasPendingReviewAsync(
            Guid caseLetterId,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            return await context.ClientResponses.AnyAsync(
                x => x.CaseLetterId == caseLetterId &&
                     (x.Status == ClientResponseStatus.Received ||
                      x.Status == ClientResponseStatus.UnderReview ||
                      x.Status == ClientResponseStatus.AdditionalInformationRequested ||
                      x.Status == ClientResponseStatus.Incomplete),
                cancellationToken);
        }

        public async Task<bool> HasResubmissionAsync(
            Guid previousResponseId,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            return await context.ClientResponses
                .AnyAsync(
                    x => x.PreviousResponseId == previousResponseId,
                    cancellationToken);
        }

        public async Task<bool> HasAnyResponseAsync(
    Guid caseLetterId,
    CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            return await context.ClientResponses
                .AnyAsync(
                    x => x.CaseLetterId == caseLetterId,
                    cancellationToken);
        }
    }
}