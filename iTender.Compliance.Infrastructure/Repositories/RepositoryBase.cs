using iTender.Compliance.Infrastructure.Data;

namespace iTender.Compliance.Infrastructure.Repositories
{
    public abstract class RepositoryBase
    {
        protected readonly ComplianceDbContext Context;

        protected RepositoryBase(
            ComplianceDbContext context)
        {
            Context = context;
        }
    }
}
