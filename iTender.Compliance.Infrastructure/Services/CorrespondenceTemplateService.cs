using iTender.Compliance.Application.DTOs;
using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Application.Interfaces.Services;
using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Infrastructure.Services
{
    public class CorrespondenceTemplateService : ICorrespondenceTemplateService
    {
        private readonly ICorrespondenceTemplateRepository _repository;
        private readonly IUnitOfWork _unitOfWork;

        public CorrespondenceTemplateService(
            ICorrespondenceTemplateRepository repository,
            IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public CorrespondenceTemplateService(
        ICorrespondenceTemplateRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<CorrespondenceTemplateModel>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<CorrespondenceTemplateModel?> GetByIdAsync(
            Guid id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<CorrespondenceTemplateModel?> GetActiveAsync(
            CorrespondenceTemplateType type)
        {
            return await _repository.GetActiveAsync(type);
        }

        public async Task<CorrespondenceTemplateModel> CreateAsync(
                CorrespondenceTemplateModel template)
        {
            template.Id = Guid.NewGuid();

            template.CreatedOn = DateTime.UtcNow;

            template.Status =
                CorrespondenceTemplateStatus.Draft;

            template.IsActive = false;

            await _repository.AddAsync(template);

            return template;
        }

        public async Task UpdateAsync(
            CorrespondenceTemplateModel template)
        {
            template.UpdatedOn = DateTime.UtcNow;

            await _repository.UpdateAsync(template);
        }
    }
}