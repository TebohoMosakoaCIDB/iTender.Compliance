using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace iTender.Compliance.Infrastructure.Repositories
{
    public class ComplianceFindingRepository : IComplianceFindingRepository
    {
        private readonly ComplianceDbContext _context;
        private readonly DbSet<ComplianceFinding> _dbSet;

        public ComplianceFindingRepository(ComplianceDbContext context)
        {
            _context = context;
            _dbSet = context.Set<ComplianceFinding>();
        }

        public async Task<ComplianceFinding?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => await _dbSet.FindAsync(new object[] { id }, cancellationToken);

        public async Task<IEnumerable<ComplianceFinding>> GetByCaseIdAsync(Guid caseId, CancellationToken cancellationToken = default)
            => await _dbSet.Where(f => f.ComplianceCaseId == caseId).ToListAsync(cancellationToken);

        public async Task AddAsync(ComplianceFinding finding, CancellationToken cancellationToken = default)
            => await _dbSet.AddAsync(finding, cancellationToken);

        public Task UpdateAsync(ComplianceFinding finding, CancellationToken cancellationToken = default)
        {
            _dbSet.Update(finding);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(ComplianceFinding finding, CancellationToken cancellationToken = default)
        {
            _dbSet.Remove(finding);
            return Task.CompletedTask;
        }
    }
}
