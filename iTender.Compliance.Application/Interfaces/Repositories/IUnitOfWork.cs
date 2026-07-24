namespace iTender.Compliance.Application.Interfaces.Repositories
{
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);
    }
}
