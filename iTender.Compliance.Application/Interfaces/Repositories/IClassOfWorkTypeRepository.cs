using iTender.Compliance.Application.DTOs;

namespace iTender.Compliance.Application.Interfaces.Repositories
{
    public interface IClassOfWorkTypeRepository
    {
        Task<IReadOnlyList<ClassOfWorkTypeModel>> GetAllAsync(CancellationToken cancellationToken = default);

        Task<ClassOfWorkTypeModel?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    }
}
