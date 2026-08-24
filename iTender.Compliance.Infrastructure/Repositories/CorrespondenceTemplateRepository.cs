using DocumentFormat.OpenXml.InkML;
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

        public async Task<List<CorrespondenceTemplateModel>> GetAllAsync()
        {
            return await Context.CorrespondenceTemplates
                .OrderBy(x => x.Type)
                .ThenByDescending(x => x.Version)
                .ToListAsync();
        }

        public async Task<CorrespondenceTemplateModel?> GetByIdAsync(
        Guid id)
        {
            return await Context.CorrespondenceTemplates
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<CorrespondenceTemplateModel?> GetActiveAsync(
            CorrespondenceTemplateType type)
        {
            return await Context.CorrespondenceTemplates
                .FirstOrDefaultAsync(x =>
                    x.Type == type &&
                    x.IsActive &&
                    x.Status == CorrespondenceTemplateStatus.Approved);
        }

        public async Task AddAsync(
        CorrespondenceTemplateModel template)
        {
            await Context.CorrespondenceTemplates
                .AddAsync(template);

            await Context.SaveChangesAsync();
        }

        public async Task UpdateAsync(
            CorrespondenceTemplateModel template)
        {
            Context.CorrespondenceTemplates.Update(template);

            await Context.SaveChangesAsync();
        }
    }
}
