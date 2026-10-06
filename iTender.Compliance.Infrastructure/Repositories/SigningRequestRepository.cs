using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Domain.Enums;
using iTender.Compliance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace iTender.Compliance.Infrastructure.Repositories
{
    public class SigningRequestRepository : RepositoryBase, ISigningRequestRepository
    {
        public SigningRequestRepository(IDbContextFactory<ComplianceDbContext> contextFactory)
            : base(contextFactory)
        {
        }

        public async Task AddAsync(
            SigningRequest request,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            await context.SigningRequests.AddAsync(request, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(
            SigningRequest request,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            context.SigningRequests.Update(request);
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<SigningRequest?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            return await context.SigningRequests
                .Include(x => x.CaseLetter)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<SigningRequest?> GetByCaseLetterAsync(
            Guid caseLetterId,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            return await context.SigningRequests
                .Include(x => x.CaseLetter)
                .FirstOrDefaultAsync(x => x.CaseLetterId == caseLetterId, cancellationToken);
        }

        public async Task<List<SigningRequest>> GetPendingAsync(
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            return await context.SigningRequests
                .Include(x => x.CaseLetter)
                .Where(x =>
                    x.Status == SigningRequestStatus.Draft ||
                    x.Status == SigningRequestStatus.Uploaded ||
                    x.Status == SigningRequestStatus.PendingSignature)
                .OrderBy(x => x.CreatedOn)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<SigningRequest>> GetRecentWithDetailsAsync(
            int take,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            return await context.SigningRequests
                .Include(x => x.CaseLetter)
                    .ThenInclude(l => l.ComplianceCase)
                        .ThenInclude(c => c.Tender)
                .OrderByDescending(x => x.CreatedOn)
                .Take(take)
                .ToListAsync(cancellationToken);
        }
    }
}