using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Domain.Enums;
using iTender.Compliance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace iTender.Compliance.Infrastructure.Repositories
{
    public class DuplicateGroupRepository : IDuplicateGroupRepository
    {
        private readonly ComplianceDbContext _context;

        public DuplicateGroupRepository(ComplianceDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(DuplicateGroup group, CancellationToken cancellationToken = default)
            => await _context.DuplicateGroups.AddAsync(group, cancellationToken);

        public Task UpdateAsync(DuplicateGroup group, CancellationToken cancellationToken = default)
        {
            _context.DuplicateGroups.Update(group);
            return Task.CompletedTask;
        }

        public async Task<DuplicateGroup?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => await _context.DuplicateGroups
                .Include(x => x.Candidates)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        public async Task<IReadOnlyList<DuplicateGroup>> GetPendingForCoAsync(CancellationToken cancellationToken = default)
            => await _context.DuplicateGroups
                .Include(x => x.Candidates)
                .Where(x => x.Status == DuplicateStatus.PendingCOVerification)
                .OrderBy(x => x.CreatedOn)
                .ToListAsync(cancellationToken);

        public async Task<bool> TenderIsInAnyPendingGroupAsync(Guid tenderId, CancellationToken cancellationToken = default)
            => await _context.DuplicateGroups
                .Where(x => x.Status == DuplicateStatus.PendingCOVerification)
                .AnyAsync(x => x.Candidates.Any(c => c.TenderId == tenderId), cancellationToken);
    }
}
