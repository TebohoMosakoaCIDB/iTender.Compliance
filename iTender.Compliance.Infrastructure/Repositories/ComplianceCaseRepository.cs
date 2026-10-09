using iTender.Compliance.Application.DTOs;
using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Application.Models.Dashboard;
using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Domain.Enums;
using iTender.Compliance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace iTender.Compliance.Infrastructure.Repositories
{
    public class ComplianceCaseRepository : RepositoryBase, IComplianceCaseRepository
    {
        public ComplianceCaseRepository(IDbContextFactory<ComplianceDbContext> contextFactory)
            : base(contextFactory)
        {
        }

        public async Task<List<ComplianceCase>> GetForAnalysisAsync(
            DateTime from,
            DateTime to,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            return await context.ComplianceCases
                .AsNoTracking()
                .Include(c => c.Tender)
                .Include(c => c.Agent)
                .Include(c => c.ComplianceFindings)
                .Include(c => c.CaseLetters)
                .Where(c => c.CreatedOn >= from && c.CreatedOn <= to)
                .ToListAsync(cancellationToken);
        }

        public async Task<DashboardAgeingChart> GetAgeingAsync(
            DateTime now,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            var openDates = await context.ComplianceCases
                .AsNoTracking()
                .Where(c => c.Status != CaseStatus.Closed)
                .Select(c => c.CreatedOn)
                .ToListAsync(cancellationToken);

            return new DashboardAgeingChart
            {
                ZeroToSeven = openDates.Count(d => (now - d).TotalDays <= 7),

                EightToFifteen = openDates.Count(d =>
                {
                    var days = (now - d).TotalDays;
                    return days > 7 && days <= 15;
                }),

                SixteenToTwentyFive = openDates.Count(d =>
                {
                    var days = (now - d).TotalDays;
                    return days > 15 && days <= 25;
                }),

                TwentySixToThirty = openDates.Count(d =>
                {
                    var days = (now - d).TotalDays;
                    return days > 25 && days <= 30;
                }),

                OverThirty = openDates.Count(d => (now - d).TotalDays > 30)
            };
        }

        public async Task<DashboardCaseMetrics> GetCaseMetricsAsync(
            DateTime thirtyDaysAgo,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            var inCorrective = await context.ComplianceCases
                .AsNoTracking()
                .CountAsync(c =>
                    c.Status == CaseStatus.ContraventionNoticeIssued,
                    cancellationToken);

            var awaitingReferral = await context.ComplianceCases
                .AsNoTracking()
                .CountAsync(c =>
                    c.Status == CaseStatus.ReferredForEnforcement,
                    cancellationToken);

            var subsequentlyCompliant = await context.ComplianceCases
                .AsNoTracking()
                .CountAsync(c =>
                    c.Outcome == ComplianceOutcome.Compliant
                    && c.ClosedDate != null
                    && c.ClosedDate >= thirtyDaysAgo,
                    cancellationToken);

            var resolvedAfterIL = await context.ComplianceCases
                .AsNoTracking()
                .CountAsync(c =>
                    c.Outcome == ComplianceOutcome.Compliant
                    && c.ClosureReason == CaseClosureReason.ClientComplied
                    && !c.CaseLetters.Any(l => l.Type == LetterType.ContraventionNotice),
                    cancellationToken);

            var resolvedAfterCN = await context.ComplianceCases
                .AsNoTracking()
                .CountAsync(c =>
                    c.Outcome == ComplianceOutcome.Compliant
                    && c.ClosureReason == CaseClosureReason.ClientComplied
                    && c.CaseLetters.Any(l => l.Type == LetterType.ContraventionNotice),
                    cancellationToken);

            return new DashboardCaseMetrics
            {
                InCorrectiveAction = inCorrective,
                AwaitingReferral = awaitingReferral,
                SubsequentlyCompliant = subsequentlyCompliant,
                ResolvedAfterIL = resolvedAfterIL,
                ResolvedAfterCN = resolvedAfterCN
            };
        }

        public async Task<IReadOnlyList<ComplianceCase>> GetInBasketAsync(
            WorkBasket basket,
            Guid? agentId,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            var query = context.ComplianceCases
                .Include(c => c.Tender)
                .Include(c => c.Agent)
                .Include(c => c.ComplianceFindings)
                .Include(c => c.CaseLetters)
                .AsQueryable();

            if (agentId.HasValue)
                query = query.Where(c => c.AgentId == agentId.Value);

            query = basket switch
            {
                WorkBasket.NewlyIdentified =>
                    query.Where(c => c.Status == CaseStatus.New && c.AgentId == null),

                WorkBasket.VerificationRequired =>
                    query.Where(c =>
                        c.Status != CaseStatus.Closed &&
                        c.ComplianceFindings.Any(f =>
                            f.VerificationStatus == VerificationStatus.AutomatedFailed &&
                            !f.IsResolved)),

                WorkBasket.NotAdvertised =>
                    query.Where(c =>
                        c.Status != CaseStatus.Closed &&
                        c.ComplianceFindings.Any(f =>
                            f.Stream == ComplianceStream.Advertisement &&
                            !f.IsResolved)),

                WorkBasket.DueForInstructionLetter =>
                    query.Where(c =>
                        c.Status == CaseStatus.New &&
                        c.ComplianceFindings.Any(f => !f.IsResolved) &&
                        !c.CaseLetters.Any(l => l.Type == LetterType.Instruction)),

                WorkBasket.DueForContraventionNotice =>
                    query.Where(c =>
                        c.Status == CaseStatus.WaitingForResponse &&
                        c.CaseLetters.Any(l =>
                            (l.Type == LetterType.Instruction || l.Type == LetterType.Reminder) &&
                            l.RespondedOn == null &&
                            l.ResponseDueOn <= DateTime.UtcNow)),

                WorkBasket.CorrectiveAction =>
                    query.Where(c => c.Status == CaseStatus.ContraventionNoticeIssued),

                WorkBasket.NonCompliant =>
                    query.Where(c =>
                        c.Status != CaseStatus.Closed &&
                        c.Status != CaseStatus.ReferredForEnforcement &&
                        c.ComplianceFindings.Any(f => !f.IsResolved)),

                WorkBasket.Referral =>
                    query.Where(c => c.Status == CaseStatus.ReferredForEnforcement),

                WorkBasket.ClearedResolved =>
                    query.Where(c => c.Status == CaseStatus.Closed),

                WorkBasket.ReferralPending =>
                    query.Where(c =>
                        c.Status == CaseStatus.ContraventionNoticeIssued &&
                        c.CaseLetters.Any(l =>
                            l.Type == LetterType.ContraventionNotice &&
                            l.RespondedOn == null &&
                            l.ResponseDueOn <= DateTime.UtcNow)),

                _ => query
            };

            return await query
                .OrderByDescending(c => c.Priority)
                .ThenBy(c => c.CreatedOn)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(
            ComplianceCase complianceCase,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            await context.ComplianceCases.AddAsync(complianceCase, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<ComplianceCase>> GetByIdsAsync(
            IEnumerable<Guid> ids,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            var idList = ids.ToList();

            return await context.ComplianceCases
                .Include(c => c.Tender)
                .Include(c => c.CaseLetters)
                .Where(c => idList.Contains(c.Id))
                .ToListAsync(cancellationToken);
        }

        public async Task UpdateAsync(
            ComplianceCase complianceCase,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            context.ComplianceCases.Update(complianceCase);
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<ComplianceCase?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            return await context.ComplianceCases
                .Include(x => x.Tender)
                .Include(x => x.Agent)
                .Include(x => x.CaseLetters)
                .Include(x => x.ComplianceFindings)
                .Include(x => x.AuditLogs)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<List<ComplianceCase>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            return await context.ComplianceCases
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedOn)
                .ToListAsync(cancellationToken);
        }

        public async Task<PagedResult<ComplianceCase>> SearchAsync(
            ComplianceCaseSearchModel search,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            var query = context.ComplianceCases
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
            await using var context = await CreateContextAsync();

            var model = await context.ComplianceCases
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
                            TenderStatusAtCheck = f.TenderStatusAtCheck,
                            VerificationStatus = f.VerificationStatus
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

            model.Timeline = await context.AuditLogs
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

        public async Task<CaseLetter?> GetLatestOutstandingAsync(
            Guid complianceCaseId,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            return await context.CaseLetters
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
            await using var context = await CreateContextAsync();

            return await context.ComplianceCases
                .CountAsync(x =>
                    x.AgentId == agentId &&
                    x.Status != CaseStatus.Closed,
                    cancellationToken);
        }

        public async Task<List<ComplianceCase>> GetCasesAwaitingReminderAsync(
            int reminderAfterHours,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            var reminderCutoff = DateTime.UtcNow.AddHours(-reminderAfterHours);

            return await context.ComplianceCases
                .Include(x => x.Agent)
                .Include(x => x.Tender)
                .Include(x => x.CaseLetters)
                .Where(x => x.Status != CaseStatus.Closed)
                .Where(x => x.AgentId != null)
                .Where(x => x.Status == CaseStatus.WaitingForResponse)
                .Where(x => x.CaseLetters.Any(l =>
                    l.Type == LetterType.Instruction &&
                    l.CreatedOn <= reminderCutoff))
                .Where(x => !x.CaseLetters.Any(l =>
                    l.Type == LetterType.Reminder))
                .OrderBy(x => x.CreatedOn)
                .ToListAsync(cancellationToken);
        }

        public async Task<ComplianceCase?> GetByTenderIdAsync(
            Guid tenderId,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            return await context.ComplianceCases
                .FirstOrDefaultAsync(c => c.TenderId == tenderId, cancellationToken);
        }

        public async Task<List<ComplianceCase>> GetOpenCasesAssignedBeforeAsync(
            DateTime cutoff,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            return await context.ComplianceCases
                .Include(x => x.Tender)
                .Include(c => c.ComplianceFindings)
                .Where(x => x.Status != CaseStatus.Closed
                         && x.Status != CaseStatus.ReferredForEnforcement)
                .Where(x => x.AssignedOn != null && x.AssignedOn <= cutoff)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<ComplianceCase>> GetContraventionNoticeDueAsync(
            int withinWorkingDays,
            CancellationToken cancellationToken = default)
        {
            await using var context = await CreateContextAsync();

            var cutoff = DateTime.UtcNow.AddDays(withinWorkingDays);

            return await context.ComplianceCases
                .Include(c => c.Tender)
                .Include(c => c.CaseLetters)
                .Where(c => c.Status == CaseStatus.WaitingForResponse
                         && c.CaseLetters.Any(l =>
                                (l.Type == LetterType.Instruction || l.Type == LetterType.Reminder)
                                && l.RespondedOn == null
                                && l.ResponseDueOn <= cutoff))
                .OrderBy(c => c.CaseLetters
                    .Where(l => l.RespondedOn == null)
                    .Max(l => l.ResponseDueOn))
                .ToListAsync(cancellationToken);
        }
    }
}