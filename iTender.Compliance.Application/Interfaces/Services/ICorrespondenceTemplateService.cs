using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Application.Interfaces.Services
{
    public interface ICorrespondenceTemplateService
    {
        Task<List<CorrespondenceTemplateModel>> GetAllAsync();

        Task<CorrespondenceTemplateModel?> GetByIdAsync(
            Guid id);

        Task<CorrespondenceTemplateModel?> GetActiveAsync(
            CorrespondenceTemplateType type);

        Task<CorrespondenceTemplateModel> CreateAsync(
            CorrespondenceTemplateModel template);

        Task UpdateAsync(
            CorrespondenceTemplateModel template);
    }
}