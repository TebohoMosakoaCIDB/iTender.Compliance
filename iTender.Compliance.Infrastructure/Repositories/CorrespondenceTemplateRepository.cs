using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Domain.Enums;
using iTender.Compliance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace iTender.Compliance.Infrastructure.Repositories
{
    public class CorrespondenceTemplateRepository
        : RepositoryBase, ICorrespondenceTemplateRepository
    {
        public CorrespondenceTemplateRepository(
            ComplianceDbContext context)
            : base(context)
        {
        }

        public async Task<List<CorrespondenceTemplateModel>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            return await Context.CorrespondenceTemplates
                .OrderBy(x => x.TemplateType)
                .ToListAsync(cancellationToken);
        }

        public async Task<CorrespondenceTemplateModel?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return await Context.CorrespondenceTemplates
                .FirstOrDefaultAsync(x =>
                    x.Id == id,
                    cancellationToken);
        }

        public async Task<CorrespondenceTemplateModel?> GetByTypeAsync(
            CorrespondenceTemplateType type,
            CancellationToken cancellationToken = default)
        {
            return await Context.CorrespondenceTemplates
                .FirstOrDefaultAsync(x =>
                    x.TemplateType == type &&
                    x.IsActive,
                    cancellationToken);
        }

        public async Task AddAsync(
            CorrespondenceTemplateModel template,
            CancellationToken cancellationToken = default)
        {
            await Context.CorrespondenceTemplates
                .AddAsync(template, cancellationToken);
        }

        public Task UpdateAsync(
            CorrespondenceTemplateModel template,
            CancellationToken cancellationToken = default)
        {
            Context.CorrespondenceTemplates.Update(template);

            return Task.CompletedTask;
        }
    }
}
