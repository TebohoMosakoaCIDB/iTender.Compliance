using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace iTender.Compliance.Infrastructure.Repositories
{
    public class ComplianceActionRepository : IComplianceActionRepository
    {
        private readonly ComplianceDbContext _context;
        private readonly DbSet<ComplianceAction> _dbSet;

        public ComplianceActionRepository(ComplianceDbContext context)
        {
            _context = context;
            _dbSet = context.Set<ComplianceAction>();
        }

        public async Task<ComplianceAction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => await _dbSet.FindAsync(new object[] { id }, cancellationToken);

        public async Task<IEnumerable<ComplianceAction>> GetByCaseIdAsync(Guid caseId, CancellationToken cancellationToken = default)
            => await _dbSet.Where(a => a.ComplianceCaseId == caseId).ToListAsync(cancellationToken);

        public async Task AddAsync(ComplianceAction action, CancellationToken cancellationToken = default)
            => await _dbSet.AddAsync(action, cancellationToken);

        public Task UpdateAsync(ComplianceAction action, CancellationToken cancellationToken = default)
        {
            _dbSet.Update(action);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(ComplianceAction action, CancellationToken cancellationToken = default)
        {
            _dbSet.Remove(action);
            return Task.CompletedTask;
        }
    }
}
