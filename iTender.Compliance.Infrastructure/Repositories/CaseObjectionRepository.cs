using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Domain.Enums;
using iTender.Compliance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace iTender.Compliance.Infrastructure.Repositories
{
    public class CaseObjectionRepository : RepositoryBase, ICaseObjectionRepository
    {
        public CaseObjectionRepository(ComplianceDbContext context)
            : base(context)
        {
        }

        public Task<CaseObjection?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return Context.CaseObjections
                .Include(x => x.ComplianceCase)
                .Include(x => x.CaseLetter)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public Task<List<CaseObjection>> GetByCaseIdAsync(
            Guid complianceCaseId,
            CancellationToken cancellationToken = default)
        {
            return Context.CaseObjections
                .Where(x => x.ComplianceCaseId == complianceCaseId)
                .Include(x => x.ComplianceCase)
                .Include(x => x.CaseLetter)
                .OrderByDescending(x => x.ReceivedOn)
                .ToListAsync(cancellationToken);
        }

        public Task<List<CaseObjection>> GetAwaitingReviewAsync(
            CancellationToken cancellationToken = default)
        {
            return Context.CaseObjections
                .Include(x => x.ComplianceCase)
                    .ThenInclude(c => c.Tender)
                .Where(x => x.Status != ObjectionStatus.Resolved)
                .OrderBy(x => x.ReceivedOn)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(
            CaseObjection objection,
            CancellationToken cancellationToken = default)
        {
            await Context.CaseObjections.AddAsync(objection, cancellationToken);
        }

        public Task UpdateAsync(
            CaseObjection objection,
            CancellationToken cancellationToken = default)
        {
            Context.CaseObjections.Update(objection);
            return Task.CompletedTask;
        }
    }
}