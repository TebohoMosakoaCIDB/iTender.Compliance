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
        public ClientGroupRepository(ComplianceDbContext context)
            : base(context)
        {
        }

        public async Task AddAsync(
            ClientGroup group,
            CancellationToken cancellationToken = default)
        {
            await Context.ClientGroups.AddAsync(group, cancellationToken);
        }

        public Task UpdateAsync(
            ClientGroup group,
            CancellationToken cancellationToken = default)
        {
            Context.ClientGroups.Update(group);
            return Task.CompletedTask;
        }

        public Task<ClientGroup?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return Context.ClientGroups
                .Include(g => g.Agent)
                .Include(g => g.Cases)
                    .ThenInclude(c => c.ComplianceCase)
                        .ThenInclude(cc => cc.Tender)
                .Include(g => g.ConsolidatedLetter)
                .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
        }

        public Task<ClientGroup?> GetDraftByEmployerAsync(
            string normalizedEmployerName,
            CancellationToken cancellationToken = default)
        {
            return Context.ClientGroups
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
            return await Context.ClientGroups
                .Include(g => g.Cases)
                .Where(g => g.NormalizedEmployerName == normalizedEmployerName)
                .OrderByDescending(g => g.CreatedOn)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<ClientGroup>> GetDraftAsync(
            CancellationToken cancellationToken = default)
        {
            return await Context.ClientGroups
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
            return await Context.ClientGroups
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
            var query = Context.ClientGroups.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search.SearchText))
            {
                var text = search.SearchText.ToLower();
                query = query.Where(g => g.EmployerName.ToLower().Contains(text));
            }

            if (search.Status.HasValue)
                query = query.Where(g => g.Status == search.Status.Value);

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
            return await Context.ClientGroups
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
            // Cases for this employer that are not already in any group.
            return await Context.ComplianceCases
                .Include(c => c.Tender)
                .Where(c => c.Tender.NormalizedEmployerName == normalizedEmployerName)
                .Where(c => !Context.ClientGroupCases.Any(g => g.ComplianceCaseId == c.Id))
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
            var group = await Context.ClientGroups
                .FirstOrDefaultAsync(g => g.Id == groupId, cancellationToken)
                ?? throw new InvalidOperationException("Client group not found.");

            group.ContactName = contactName;
            group.ContactEmail = contactEmail;
            group.CcEmails = ccEmails;
            group.ModifiedBy = modifiedByUserId;
            group.ModifiedOn = DateTime.UtcNow;
        }
    }
}