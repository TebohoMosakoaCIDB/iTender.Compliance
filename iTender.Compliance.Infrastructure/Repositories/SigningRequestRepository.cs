using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Domain.Enums;
using iTender.Compliance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace iTender.Compliance.Infrastructure.Repositories
{
    public class SigningRequestRepository : RepositoryBase, ISigningRequestRepository
    {
        public SigningRequestRepository(ComplianceDbContext context)
            : base(context)
        {
        }

        public async Task AddAsync(
            SigningRequest request,
            CancellationToken cancellationToken = default)
        {
            await Context.SigningRequests.AddAsync(
                request,
                cancellationToken);
        }

        public Task UpdateAsync(
            SigningRequest request,
            CancellationToken cancellationToken = default)
        {
            Context.SigningRequests.Update(request);

            return Task.CompletedTask;
        }

        public Task<SigningRequest?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return Context.SigningRequests
                .Include(x => x.CaseLetter)
                .FirstOrDefaultAsync(
                    x => x.Id == id,
                    cancellationToken);
        }

        public Task<SigningRequest?> GetByCaseLetterAsync(
            Guid caseLetterId,
            CancellationToken cancellationToken = default)
        {
            return Context.SigningRequests
                .Include(x => x.CaseLetter)
                .FirstOrDefaultAsync(
                    x => x.CaseLetterId == caseLetterId,
                    cancellationToken);
        }

        public async Task<List<SigningRequest>> GetPendingAsync(
            CancellationToken cancellationToken = default)
        {
            return await Context.SigningRequests
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
            return await Context.SigningRequests
                .Include(x => x.CaseLetter)
                    .ThenInclude(l => l.ComplianceCase)
                        .ThenInclude(c => c.Tender)
                .OrderByDescending(x => x.CreatedOn)
                .Take(take)
                .ToListAsync(cancellationToken);
        }
    }
}