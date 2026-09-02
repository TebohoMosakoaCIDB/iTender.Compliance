using DocumentFormat.OpenXml.InkML;
using iTender.Compliance.Application.DTOs;
using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Domain.Enums;
using iTender.Compliance.Infrastructure.Data;
using iTender.Compliance.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace iTender.Compliance.Infrastructure.Repositories
{
    public class ComplianceCaseRepository
    : RepositoryBase, IComplianceCaseRepository
    {
        public ComplianceCaseRepository(
            ComplianceDbContext context)
            : base(context)
        {
        }

        public async Task AddAsync(
            ComplianceCase complianceCase,
            CancellationToken cancellationToken = default)
        {
            await Context.ComplianceCases.AddAsync(
                complianceCase,
                cancellationToken);
        }

        public Task UpdateAsync(
            ComplianceCase complianceCase,
            CancellationToken cancellationToken = default)
        {
            Context.ComplianceCases.Update(complianceCase);

            return Task.CompletedTask;
        }

        public Task<ComplianceCase?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return Context.ComplianceCases
                .Include(x => x.Tender)
                .Include(x => x.Agent)
                .Include(x => x.CaseLetters)
                .Include(x => x.AuditLogs)
                .FirstOrDefaultAsync(
                    x => x.Id == id,
                    cancellationToken);
        }

        public Task<List<ComplianceCase>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            return Context.ComplianceCases
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedOn)
                .ToListAsync(cancellationToken);
        }

        public async Task<PagedResult<ComplianceCase>> SearchAsync(
    ComplianceCaseSearchModel search,
    CancellationToken cancellationToken = default)
        {
            var query = Context.ComplianceCases
                .Include(x => x.Tender)
                .Include(x => x.Agent)
                .Include(c => c.ComplianceFindings)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search.SearchText))
            {
                query = query.Where(x =>
                    x.Tender.TenderNumber.Contains(search.SearchText) ||
                    x.Tender.Title.Contains(search.SearchText) ||
                    x.Tender.EmployerName.Contains(search.SearchText));
            }

            if (search.Status.HasValue)
                query = query.Where(x => x.Status == search.Status);

            if (search.Priority.HasValue)
                query = query.Where(x => x.Priority == search.Priority);

            if (search.AgentId.HasValue)
                query = query.Where(x => x.AgentId == search.AgentId);

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip((search.PageNumber - 1) * search.PageSize)
                .Take(search.PageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<ComplianceCase>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = search.PageNumber,
                PageSize = search.PageSize
            };

        }

        public async Task<ComplianceCaseDetailModel?> GetDetailAsync(
    Guid id,
    CancellationToken cancellationToken = default)
        {
            var model = await Context.ComplianceCases
                .Where(x => x.Id == id)
                .Select(x => new ComplianceCaseDetailModel
                {
                    Id = x.Id,

                    Tender = new TenderDetailModel
                    {
                        Id = x.Tender.Id,
                        TenderNumber = x.Tender.TenderNumber,
                        Title = x.Tender.Title,
                        Employer = x.Tender.EmployerName,
                        ContactName = x.Tender.ContactName,
                        ContactEmail = x.Tender.ContactEmail,
                        ClosingDate = x.Tender.ClosingDate,
                        TenderUrl = x.Tender.TenderUrl
                    },

                    Case = new CaseDetailModel
                    {
                        Status = x.Status.ToString(),
                        Priority = x.Priority,
                        Outcome = x.Outcome.HasValue
                            ? x.Outcome.Value.ToString()
                            : null,
                        AgentId = x.AgentId,
                        // ---- Null‑safe access for Agent ----
                        Level = x.Agent != null ? x.Agent.Level : (AgentLevel?)null,
                        JobTitle = x.Agent != null ? x.Agent.JobTitle : null,
                        FooterText = x.Agent != null ? x.Agent.FooterText : null,
                        Agent = x.Agent != null ? x.Agent.FullName : null,
                        AgentEmail = x.Agent != null ? x.Agent.Email : null,
                        CreatedOn = x.CreatedOn,
                        ClosedOn = x.ClosedDate,
                        Comments = x.Comments
                    },

                    Letters = x.CaseLetters
                        .OrderBy(l => l.LetterNumber)
                        .Select(l => new CaseLetterModel
                        {
                            Id = l.Id,
                            LetterNumber = l.LetterNumber,
                            Type = l.Type,
                            SentOn = l.SentOn,
                            RecipientName = l.RecipientName,
                            RecipientEmail = l.RecipientEmail,
                            ResponseDueOn = l.ResponseDueOn,
                            RespondedOn = l.RespondedOn,
                            Accepted = l.Accepted,
                            FileName = l.FileName
                        })
                        .ToList(),

                    Notes = x.CaseNotes
                        .OrderByDescending(n => n.CreatedOn)
                        .Select(n => new CaseNoteModel
                        {
                            Id = n.Id,
                            Note = n.Comment,
                            CreatedOn = n.CreatedOn
                        })
                        .ToList(),

                    Findings = x.ComplianceFindings
                        .Select(f => new ComplianceFindingDto
                        {
                            Id = f.Id,
                            Stream = f.Stream,
                            FindingType = f.FindingType,
                            Description = f.Description,
                            RegulatoryReference = f.RegulatoryReference,
                            IdentifiedAt = f.IdentifiedAt,
                            IsResolved = f.IsResolved,
                            ResolvedOn = f.ResolvedOn,
                            TenderStatusAtCheck = f.TenderStatusAtCheck
                        })
                        .ToList(),

                    Actions = x.ComplianceActions
                        .Select(a => new ComplianceActionDto
                        {
                            Id = a.Id,
                            ActionType = a.ActionType,
                            Status = a.Status,
                            ActionDate = a.ActionDate,
                            ResponseDueDate = a.ResponseDueDate,
                            CompletedDate = a.CompletedDate,
                            Comments = a.Comments
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (model == null)
                return null;

            model.Timeline = await Context.AuditLogs
                .Where(x =>
                    x.Entity == AuditEntity.ComplianceCase &&
                    x.EntityId == id)
                .OrderByDescending(x => x.CreatedOn)
                .Select(x => new AuditLogModel
                {
                    Date = x.CreatedOn,
                    User = x.CreatedBy != null
                                ? x.CreatedBy.ToString()!
                                : "System",
                    Action = x.Action,
                    Description = x.Description
                })
                .ToListAsync(cancellationToken);

            return model;
        }

        public Task<CaseLetter?> GetLatestOutstandingAsync(
    Guid complianceCaseId,
    CancellationToken cancellationToken = default)
        {
            return Context.CaseLetters
                .Where(x =>
                    x.ComplianceCaseId == complianceCaseId &&
                    !x.RespondedOn.HasValue)
                .OrderByDescending(x => x.LetterNumber)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<int> GetOpenCaseCountByAgentAsync(
    Guid agentId,
    CancellationToken cancellationToken = default)
        {
            return await Context.ComplianceCases
                .CountAsync(x =>
                    x.AgentId == agentId &&
                    x.Status != CaseStatus.Closed,
                    cancellationToken);
        }

        public async Task<IEnumerable<ComplianceCase>> GetOverdueCasesAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            return await Context.ComplianceCases
                .Include(c => c.CaseLetters) // to get the letters
                .Where(c => (c.Status == CaseStatus.WaitingForResponse || c.Status == CaseStatus.WaitingForResponse)
                            && c.CaseLetters.Any(l => l.ResponseDueOn < now && l.RespondedOn == null))
                .ToListAsync(cancellationToken);
        }

        public async Task<List<ComplianceCase>> GetCasesAwaitingReminderAsync(
    int reminderAfterHours,
    CancellationToken cancellationToken = default)
        {
            var reminderCutoff = DateTime.UtcNow.AddHours(-reminderAfterHours);

            return await Context.ComplianceCases

                .Include(x => x.Agent)
                .Include(x => x.Tender)
                .Include(x => x.CaseLetters)

                // Case must still be open
                .Where(x => x.Status != CaseStatus.Closed)

                // Must be assigned
                .Where(x => x.AgentId != null)

                // Waiting for supplier response
                .Where(x => x.Status == CaseStatus.WaitingForResponse)

                // Must have an instruction letter older than X hours
                .Where(x => x.CaseLetters.Any(l =>
                    l.Type == LetterType.Instruction &&
                    l.CreatedOn <= reminderCutoff))

                // Must NOT already have a reminder
                .Where(x => !x.CaseLetters.Any(l =>
                    l.Type == LetterType.Reminder))

                .OrderBy(x => x.CreatedOn)

                .ToListAsync(cancellationToken);
        }

        public async Task<ComplianceCase?> GetByTenderIdAsync(Guid tenderId, CancellationToken cancellationToken = default)
        {
            return await Context.ComplianceCases.FirstOrDefaultAsync(c => c.TenderId == tenderId, cancellationToken);
        }

        public async Task<List<ComplianceCase>> GetOpenCasesAssignedBeforeAsync(
            DateTime cutoff,
            CancellationToken cancellationToken = default)
        {
            return await Context.ComplianceCases
                .Include(x => x.Tender)
                .Where(x => x.Status != CaseStatus.Closed && x.Status != CaseStatus.ReferredForEnforcement)
                .Where(x => x.AssignedOn != null && x.AssignedOn <= cutoff)
                .ToListAsync(cancellationToken);
        }
    }
}