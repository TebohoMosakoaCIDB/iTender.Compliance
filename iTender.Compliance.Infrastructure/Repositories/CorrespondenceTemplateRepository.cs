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
        public CorrespondenceTemplateRepository(IDbContextFactory<ComplianceDbContext> contextFactory)
            : base(contextFactory)
        {
        }

        public async Task<List<CorrespondenceTemplateModel>> GetAllAsync()
        {
            await using var context = await CreateContextAsync();

            return await context.CorrespondenceTemplates
                .OrderBy(x => x.Type)
                .ThenByDescending(x => x.Version)
                .ToListAsync();
        }

        public async Task<CorrespondenceTemplateModel?> GetByIdAsync(Guid id)
        {
            await using var context = await CreateContextAsync();

            return await context.CorrespondenceTemplates
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<CorrespondenceTemplateModel?> GetActiveAsync(
            CorrespondenceTemplateType type)
        {
            await using var context = await CreateContextAsync();

            return await context.CorrespondenceTemplates
                .FirstOrDefaultAsync(x =>
                    x.Type == type &&
                    x.IsActive &&
                    x.Status == CorrespondenceTemplateStatus.Approved);
        }

        public async Task AddAsync(CorrespondenceTemplateModel template)
        {
            await using var context = await CreateContextAsync();

            await context.CorrespondenceTemplates.AddAsync(template);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(CorrespondenceTemplateModel template)
        {
            await using var context = await CreateContextAsync();

            context.CorrespondenceTemplates.Update(template);
            await context.SaveChangesAsync();
        }
    }
}