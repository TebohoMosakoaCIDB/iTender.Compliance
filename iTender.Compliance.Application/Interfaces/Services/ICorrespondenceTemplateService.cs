using iTender.Compliance.Application.DTOs;
using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Application.Interfaces.Services
{
    public interface ICorrespondenceTemplateService
    {
        Task<List<CorrespondenceTemplateListModel>> GetAllAsync(
            CancellationToken cancellationToken = default);

        Task<CorrespondenceTemplateModel?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task UpdateAsync(
            SaveCorrespondenceTemplateModel model,
            CancellationToken cancellationToken = default);

        Task<Guid> CreateAsync(
            CorrespondenceTemplateType type,
            CancellationToken cancellationToken = default);
    }
}