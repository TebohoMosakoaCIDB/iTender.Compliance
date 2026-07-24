using iTender.Compliance.Application.Interfaces.Repositories;

namespace iTender.Compliance.Infrastructure.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ComplianceDbContext _context;

        public UnitOfWork(ComplianceDbContext context)
        {
            _context = context;
        }

        public Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            return _context.SaveChangesAsync(cancellationToken);
        }
    }
}
