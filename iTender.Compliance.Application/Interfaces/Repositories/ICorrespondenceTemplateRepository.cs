using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Application.Interfaces.Repositories
{
    public interface ICorrespondenceTemplateRepository
    {
        Task<List<CorrespondenceTemplateModel>> GetAllAsync(
            CancellationToken cancellationToken = default);

        Task<CorrespondenceTemplateModel?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task<CorrespondenceTemplateModel?> GetByTypeAsync(
            CorrespondenceTemplateType type,
            CancellationToken cancellationToken = default);

        Task AddAsync(
            CorrespondenceTemplateModel template,
            CancellationToken cancellationToken = default);

        Task UpdateAsync(
            CorrespondenceTemplateModel template,
            CancellationToken cancellationToken = default);
    }
}
