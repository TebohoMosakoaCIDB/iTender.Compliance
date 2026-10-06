using iTender.Compliance.Application.Interfaces.Repositories;

namespace iTender.Compliance.Infrastructure.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            // No-op.
            //
            // Repositories now create a per-operation DbContext and call
            // SaveChangesAsync inside their own methods. There is no shared
            // context to flush, so this is intentionally a no-op.
            //
            // Existing callers can keep calling SaveChangesAsync - it's
            // harmless and will be removed in a future cleanup.
            return Task.FromResult(0);
        }
    }
}