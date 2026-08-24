using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Application.Interfaces.Repositories
{
    public interface ICorrespondenceTemplateRepository
    {
        Task<List<CorrespondenceTemplateModel>> GetAllAsync();

        Task<CorrespondenceTemplateModel?> GetByIdAsync(
            Guid id);

        Task<CorrespondenceTemplateModel?> GetActiveAsync(
            CorrespondenceTemplateType type);

        Task AddAsync(
            CorrespondenceTemplateModel template);

        Task UpdateAsync(
            CorrespondenceTemplateModel template);
    }
}
