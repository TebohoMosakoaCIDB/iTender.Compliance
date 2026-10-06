using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace iTender.Compliance.Infrastructure.Repositories
{
    public class CaseNoteRepository : RepositoryBase, ICaseNoteRepository
    {
        public CaseNoteRepository(IDbContextFactory<ComplianceDbContext> contextFactory)
            : base(contextFactory)
        {
        }

        public async Task AddAsync(
            CaseNote note,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            await context.Notes.AddAsync(note, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<CaseNote>> GetByCaseIdAsync(
            Guid complianceCaseId,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            return await context.Notes
                .Where(x => x.ComplianceCaseId == complianceCaseId)
                .OrderByDescending(x => x.CreatedOn)
                .ToListAsync(cancellationToken);
        }
    }
}