using iTender.Compliance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace iTender.Compliance.Infrastructure.Repositories
{
    public abstract class RepositoryBase
    {
        private readonly IDbContextFactory<ComplianceDbContext> _contextFactory;
        private ComplianceDbContext? _context;

        protected RepositoryBase(
            IDbContextFactory<ComplianceDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        /// <summary>
        /// Lazy-created context, reused within a single repository instance.
        /// Prefer CreateContextAsync() for new code where the context lifecycle
        /// needs to be explicit.
        /// </summary>
        protected ComplianceDbContext Context
            => _context ??= _contextFactory.CreateDbContext();

        /// <summary>
        /// Creates a new DbContext scoped to the caller. Use in new code
        /// so each operation gets its own context and races across
        /// navigation are impossible.
        /// </summary>
        protected Task<ComplianceDbContext> CreateContextAsync()
            => _contextFactory.CreateDbContextAsync();
    }
}
