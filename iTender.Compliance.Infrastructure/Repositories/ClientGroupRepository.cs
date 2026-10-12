using iTender.Compliance.Application.DTOs;
using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Domain.Enums;
using iTender.Compliance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace iTender.Compliance.Infrastructure.Repositories
{
    public class ClientGroupRepository : RepositoryBase, IClientGroupRepository
    {
        public ClientGroupRepository(IDbContextFactory<ComplianceDbContext> contextFactory)
            : base(contextFactory)
        {
        }

        public async Task<ClientGroupSummaryModel?> GetSummaryByCaseIdAsync(
            Guid caseId,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            return await context.ClientGroupCases
                .Where(gc => gc.ComplianceCaseId == caseId)
                .Select(gc => new ClientGroupSummaryModel
                {
                    Id = gc.ClientGroup.Id,
                    EmployerName = gc.ClientGroup.EmployerName,
                    Status = gc.ClientGroup.Status,
                    TenderCount = gc.ClientGroup.Cases.Count,
                    AgentName = gc.ClientGroup.Agent != null
                        ? gc.ClientGroup.Agent.FullName
                        : null
                })
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task AddAsync(
            ClientGroup group,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            await context.ClientGroups.AddAsync(group, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<int> GetOpenGroupCountByAgentAsync(
            Guid agentId,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            return await context.ClientGroups
                .CountAsync(g => g.AgentId == agentId
                              && g.Status != ClientGroupStatus.Closed,
                             cancellationToken);
        }

        public async Task UpdateAsync(
            ClientGroup group,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            context.ClientGroups.Update(group);
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<ClientGroup?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            return await context.ClientGroups
                .Include(g => g.Agent)
                .Include(g => g.Cases)
                    .ThenInclude(c => c.ComplianceCase)
                        .ThenInclude(cc => cc.Tender)
                .Include(g => g.ConsolidatedLetter)
                .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
        }

        public async Task<IReadOnlyList<ClientGroup>> GetByAgentAsync(
            Guid agentId,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            return await context.ClientGroups
                .Include(g => g.Cases)
                    .ThenInclude(c => c.ComplianceCase)
                        .ThenInclude(cc => cc.Tender)
                .Where(g => g.AgentId == agentId
                         && g.Status != ClientGroupStatus.Closed)
                .OrderBy(g => g.Status)
                .ThenByDescending(g => g.CreatedOn)
                .ToListAsync(cancellationToken);
        }

        public async Task<ClientGroup?> GetDraftByEmployerAsync(
            string normalizedEmployerName,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            return await context.ClientGroups
                .Include(g => g.Cases)
                .FirstOrDefaultAsync(
                    g => g.NormalizedEmployerName == normalizedEmployerName
                      && g.Status == ClientGroupStatus.Draft,
                    cancellationToken);
        }

        public async Task<IReadOnlyList<ClientGroup>> GetByEmployerAsync(
            string normalizedEmployerName,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            return await context.ClientGroups
                .Include(g => g.Cases)
                .Where(g => g.NormalizedEmployerName == normalizedEmployerName)
                .OrderByDescending(g => g.CreatedOn)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<ClientGroup>> GetDraftAsync(
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            return await context.ClientGroups
                .Include(g => g.Cases)
                    .ThenInclude(c => c.ComplianceCase)
                        .ThenInclude(cc => cc.Tender)
                .Where(g => g.Status == ClientGroupStatus.Draft)
                .OrderBy(g => g.EmployerName)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<ClientGroup>> GetIssuedAsync(
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            return await context.ClientGroups
                .Include(g => g.Cases)
                    .ThenInclude(c => c.ComplianceCase)
                        .ThenInclude(cc => cc.Tender)
                .Include(g => g.ConsolidatedLetter)
                .Where(g => g.Status == ClientGroupStatus.Issued
                         || g.Status == ClientGroupStatus.LetterGenerated)
                .OrderByDescending(g => g.IssuedOn)
                .ToListAsync(cancellationToken);
        }

        public async Task<PagedResult<ClientGroupListItemModel>> SearchAsync(
            ClientGroupSearchModel search,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            var query = context.ClientGroups.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search.SearchText))
            {
                var text = search.SearchText.ToLower();
                query = query.Where(g => g.EmployerName.ToLower().Contains(text));
            }

            if (search.Status.HasValue)
                query = query.Where(g => g.Status == search.Status.Value);

            if (search.AgentId.HasValue)
            {
                query = query.Where(g => g.AgentId == search.AgentId.Value);
            }

            var total = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(g => g.CreatedOn)
                .Skip((search.PageNumber - 1) * search.PageSize)
                .Take(search.PageSize)
                .Select(g => new ClientGroupListItemModel
                {
                    Id = g.Id,
                    EmployerName = g.EmployerName,
                    Status = g.Status,
                    CaseCount = g.Cases.Count,
                    CreatedOn = g.CreatedOn,
                    IssuedOn = g.IssuedOn,
                    LetterType = g.ConsolidatedLetter != null
                        ? g.ConsolidatedLetter.Type
                        : (LetterType?)null
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<ClientGroupListItemModel>
            {
                Items = items,
                TotalCount = total,
                PageNumber = search.PageNumber,
                PageSize = search.PageSize
            };
        }

        public async Task<ClientGroupDetailModel?> GetDetailAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            return await context.ClientGroups
                .Include(g => g.Agent)
                .Where(g => g.Id == id)
                .Select(g => new ClientGroupDetailModel
                {
                    Id = g.Id,
                    EmployerName = g.EmployerName,
                    Status = g.Status,
                    ContactName = g.ContactName,
                    ContactEmail = g.ContactEmail,
                    CcEmails = g.CcEmails,
                    CreatedOn = g.CreatedOn,
                    IssuedOn = g.IssuedOn,
                    AnnexureFileName = g.AnnexureFileName,
                    AnnexureFilePath = g.AnnexureFilePath,
                    AgentId = g.AgentId,
                    AgentName = g.Agent != null ? g.Agent.FullName : null,
                    AgentEmail = g.Agent != null ? g.Agent.Email : null,
                    ConsolidatedLetterId = g.ConsolidatedLetterId,
                    ConsolidatedLetterFileName = g.ConsolidatedLetter != null
                        ? g.ConsolidatedLetter.FileName
                        : null,
                    ConsolidatedLetterFilePath = g.ConsolidatedLetter != null
                        ? g.ConsolidatedLetter.FilePath
                        : null,
                    LetterType = g.ConsolidatedLetter != null
                        ? g.ConsolidatedLetter.Type
                        : (LetterType?)null,

                    Tenders = g.Cases
                        .OrderBy(c => c.AddedOn)
                        .Select(c => new ClientGroupTenderModel
                        {
                            ComplianceCaseId = c.ComplianceCaseId,
                            TenderId = c.ComplianceCase.TenderId,
                            TenderNumber = c.ComplianceCase.Tender.TenderNumber,
                            Title = c.ComplianceCase.Tender.Title,
                            IsAdvertised = c.ComplianceCase.Tender.IsAdvertised,
                            IsRegistered = c.ComplianceCase.Tender.IsRegisteredOnRoP,
                            ClosingDate = c.ComplianceCase.Tender.ClosingDate
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<List<ComplianceCase>> GetEligibleCasesForEmployerAsync(
            string normalizedEmployerName,
            Guid excludeGroupId,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            return await context.ComplianceCases
                .Include(c => c.Tender)
                .Where(c => c.Tender.NormalizedEmployerName == normalizedEmployerName)
                .Where(c => !context.ClientGroupCases
                    .Any(g => g.ComplianceCaseId == c.Id
                           && g.ClientGroupId != excludeGroupId))
                .OrderByDescending(c => c.CreatedOn)
                .ToListAsync(cancellationToken);
        }

        public async Task UpdateGroupMetaAsync(
            Guid groupId,
            string? contactName,
            string? contactEmail,
            string? ccEmails,
            Guid modifiedByUserId,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            var group = await context.ClientGroups
                .FirstOrDefaultAsync(g => g.Id == groupId, cancellationToken)
                ?? throw new InvalidOperationException("Client group not found.");

            group.ContactName = contactName;
            group.ContactEmail = contactEmail;
            group.CcEmails = ccEmails;
            group.ModifiedBy = modifiedByUserId;
            group.ModifiedOn = DateTime.UtcNow;

            await context.SaveChangesAsync(cancellationToken);
        }

        // ClientGroupRepository
        public async Task AddCaseLinkAsync(
            ClientGroupCase link,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            await context.ClientGroupCases.AddAsync(link, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<bool> CaseLinkExistsAsync(
            Guid groupId,
            Guid caseId,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            return await context.ClientGroupCases
                .AnyAsync(gc => gc.ClientGroupId == groupId
                             && gc.ComplianceCaseId == caseId,
                          cancellationToken);
        }
    }
}