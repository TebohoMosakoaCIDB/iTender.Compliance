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

            var now = DateTime.UtcNow;

            var query = context.ComplianceCases
                .AsNoTracking()
                .Include(c => c.Tender)
                .Include(c => c.Agent)
                .Include(c => c.ComplianceFindings)
                .Include(c => c.CaseLetters)
                .AsQueryable();

            // Apply officer filtering to assigned work.
            // Unassigned cases remain visible in the Newly Identified basket.
            if (agentId.HasValue && basket != WorkBasket.NewlyIdentified)
            {
                query = query.Where(c => c.AgentId == agentId.Value);
            }

            query = basket switch
            {
                // 1. New cases that have not been allocated.
                WorkBasket.NewlyIdentified =>
                    query.Where(c =>
                        c.Status == CaseStatus.New &&
                        c.AgentId == null),

                // 2. Findings requiring manual verification.
                WorkBasket.VerificationRequired =>
                    query.Where(c =>
                        c.Status != CaseStatus.Closed &&
                        c.Status != CaseStatus.ReferredForEnforcement &&
                        c.ComplianceFindings.Any(f =>
                            f.VerificationStatus == VerificationStatus.AutomatedFailed &&
                            !f.IsResolved)),

                // 3. Advertisement findings.
                WorkBasket.NotAdvertised =>
                    query.Where(c =>
                        c.Status != CaseStatus.Closed &&
                        c.Status != CaseStatus.ReferredForEnforcement &&
                        c.ComplianceFindings.Any(f =>
                            f.Stream == ComplianceStream.Advertisement &&
                            !f.IsResolved)),

                // 4. Cases requiring an Instructional Letter.
                WorkBasket.DueForInstructionLetter =>
                    query.Where(c =>
                        (c.Status == CaseStatus.New ||
                         c.Status == CaseStatus.Assigned ||
                         c.Status == CaseStatus.InProgress) &&
                        c.ComplianceFindings.Any(f => !f.IsResolved) &&
                        !c.CaseLetters.Any(l =>
                            l.Type == LetterType.Instruction)),

                // 5. Instructional Letter response deadline has passed.
                WorkBasket.DueForContraventionNotice =>
                    query.Where(c =>
                        c.Status == CaseStatus.WaitingForResponse &&
                        c.CaseLetters.Any(l =>
                            (l.Type == LetterType.Instruction ||
                             l.Type == LetterType.Reminder) &&
                            l.RespondedOn == null &&
                            l.ResponseDueOn <= now)),

                // 6. Contravention Notice issued and corrective period
                // is still running.
                WorkBasket.CorrectiveAction =>
                    query.Where(c =>
                        c.Status == CaseStatus.ContraventionNoticeIssued &&
                        c.CaseLetters.Any(l =>
                            l.Type == LetterType.ContraventionNotice &&
                            l.Accepted != true &&
                            l.ResponseDueOn > now)),

                // 7. Cases formally classified as non-compliant.
                // This is a classification basket, not a duplicate
                // of every case with an unresolved finding.
                WorkBasket.NonCompliant =>
                    query.Where(c =>
                        c.Status != CaseStatus.Closed &&
                        c.Status != CaseStatus.ReferredForEnforcement &&
                        c.Outcome == ComplianceOutcome.NonCompliant),

                // 8. Cases already referred.
                WorkBasket.Referral =>
                    query.Where(c =>
                        c.Status == CaseStatus.ReferredForEnforcement),

                // 9. Cases that have been cleared or closed.
                WorkBasket.ClearedResolved =>
                    query.Where(c =>
                        c.Status == CaseStatus.Closed ||
                        c.Status == CaseStatus.Cleared),

                // 10. Contravention period has expired and the case
                // requires referral assessment.
                WorkBasket.ReferralPending =>
                    query.Where(c =>
                        c.Status == CaseStatus.ContraventionNoticeIssued &&
                        c.CaseLetters.Any(l =>
                            l.Type == LetterType.ContraventionNotice &&
                            l.Accepted != true &&
                            l.ResponseDueOn <= now) &&
                        c.ComplianceFindings.Any(f => !f.IsResolved)),

                _ => query.Where(c => false)
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

        public async Task<IReadOnlyList<ClientGroupedReportRow>> GetClientGroupedReportAsync(
    Guid? agentId,
    CancellationToken ct = default)
        {
            await using var context = await CreateContextAsync();

            // Start from tenders that are construction-related, group by employer.
            var query = context.Tenders
                .AsNoTracking()
                .Where(t => t.IsConstruction);

            if (agentId.HasValue)
            {
                // Only include tenders whose case is assigned to this officer.
                query = query.Where(t =>
                    t.ComplianceCase != null &&
                    t.ComplianceCase.AgentId == agentId.Value);
            }

            // Load the shape we need, then group in memory. EF Core's GroupBy with
            // complex projections over navigations is fragile on SQL Server; doing
            // the aggregation client-side is safer and fast enough for a few
            // thousand tenders.
            var rows = await query
                .Select(t => new
                {
                    t.Id,
                    t.EmployerName,
                    t.NormalizedEmployerName,
                    t.IsRegisteredOnRoP,
                    CaseStatus = t.ComplianceCase != null
                        ? (CaseStatus?)t.ComplianceCase.Status
                        : null,
                    CaseOutcome = t.ComplianceCase != null
                        ? (ComplianceOutcome?)t.ComplianceCase.Outcome
                        : null,
                    CaseAgentId = t.ComplianceCase != null
                        ? t.ComplianceCase.AgentId
                        : null,
                    AgentName = t.ComplianceCase != null && t.ComplianceCase.Agent != null
                        ? t.ComplianceCase.Agent.FullName
                        : null,
                    Letters = t.ComplianceCase != null
                        ? t.ComplianceCase.CaseLetters
                            .Select(l => new { l.Type, l.SentOn })
                            .ToList()
                        : null
                })
                .ToListAsync(ct);

            // Fetch the group for each employer (for group status + owner).
            var employers = rows
                .Where(r => !string.IsNullOrWhiteSpace(r.NormalizedEmployerName))
                .Select(r => r.NormalizedEmployerName)
                .Distinct()
                .ToList();

            var groups = await context.ClientGroups
                .AsNoTracking()
                .Where(g => employers.Contains(g.NormalizedEmployerName))
                .Select(g => new
                {
                    g.Id,
                    g.NormalizedEmployerName,
                    g.Status,
                    g.AgentId
                })
                .ToListAsync(ct);

            var groupsByEmployer = groups
                .GroupBy(g => g.NormalizedEmployerName)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.Id).First());

            var result = rows
                .GroupBy(r => r.NormalizedEmployerName)
                .Select(grp =>
                {
                    var first = grp.First();
                    groupsByEmployer.TryGetValue(grp.Key, out var group);

                    var cases = grp.Where(x => x.CaseStatus.HasValue).ToList();

                    var allLetters = grp
                        .Where(x => x.Letters != null)
                        .SelectMany(x => x.Letters!)
                        .ToList();

                    var lastLetter = allLetters
                        .OrderByDescending(l => l.SentOn)
                        .FirstOrDefault();

                    return new ClientGroupedReportRow
                    {
                        GroupId = group?.Id,
                        EmployerName = first.EmployerName,

                        TotalTenders = grp.Count(),
                        CompliantTenders = grp.Count(x => x.CaseOutcome == ComplianceOutcome.Compliant),
                        NonCompliantTenders = grp.Count(x => x.CaseOutcome == ComplianceOutcome.NonCompliant),
                        OpenTenders = grp.Count(x => !x.CaseStatus.HasValue || x.CaseStatus != CaseStatus.Closed),

                        CasesCleared = cases.Count(x => x.CaseStatus == CaseStatus.Cleared),
                        CasesClosed = cases.Count(x => x.CaseStatus == CaseStatus.Closed),
                        CasesNewlyIdentified = cases.Count(x => x.CaseStatus == CaseStatus.NewlyIdentified),
                        CasesVerificationRequired = cases.Count(x => x.CaseStatus == CaseStatus.VerificationRequired),
                        CasesNotAdvertised = cases.Count(x => x.CaseStatus == CaseStatus.NotAdvertised),

                        CasesContraventionNoticeIssued = cases.Count(x => x.CaseStatus == CaseStatus.ContraventionNoticeIssued),
                        CasesWaitingForResponse = cases.Count(x => x.CaseStatus == CaseStatus.WaitingForResponse),
                        CasesReferredForEnforcement = cases.Count(x => x.CaseStatus == CaseStatus.ReferredForEnforcement),

                        LettersIssued = allLetters.Count,
                        LastCorrespondenceDate = lastLetter?.SentOn,

                        GroupStatus = group?.Status,
                        AgentId = group?.AgentId ?? grp.Select(x => x.CaseAgentId).FirstOrDefault(x => x.HasValue),
                        AgentName = grp.Select(x => x.AgentName).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x))
                    };
                })
                .OrderByDescending(r => r.NonCompliantTenders)
                .ThenBy(r => r.EmployerName)
                .ToList();

            return result;
        }

        public async Task<IReadOnlyList<NonCompliantCaseReportRow>> GetNonCompliantCasesReportAsync(
    NonCompliantCaseReportFilter filter,
    int? slaDays = null,
    int? slaWarningDays = null,
    CancellationToken ct = default)
        {
            await using var context = await CreateContextAsync();

            var now = DateTime.UtcNow;
            var sla = slaDays ?? 30;
            var warning = slaWarningDays ?? 5;

            var query = context.ComplianceCases
                .AsNoTracking()
                .Include(c => c.Tender)
                .Include(c => c.Agent)
                .Include(c => c.ComplianceFindings)
                .Include(c => c.CaseLetters)
                .AsQueryable();

            query = query.Where(c =>
                c.Status != CaseStatus.Closed &&
                c.Status != CaseStatus.Cleared &&
                c.ComplianceFindings.Any(f => !f.IsResolved));

            if (filter.AgentId.HasValue)
                query = query.Where(c => c.AgentId == filter.AgentId.Value);

            if (!string.IsNullOrWhiteSpace(filter.EmployerName))
                query = query.Where(c =>
                    c.Tender.EmployerName.Contains(filter.EmployerName));

            if (!string.IsNullOrWhiteSpace(filter.Province))
                query = query.Where(c => c.Tender.Province == filter.Province);

            if (filter.Status.HasValue)
                query = query.Where(c => c.Status == filter.Status.Value);

            if (filter.FromDate.HasValue)
                query = query.Where(c => c.CreatedOn >= filter.FromDate.Value);

            if (filter.ToDate.HasValue)
                query = query.Where(c => c.CreatedOn <= filter.ToDate.Value);

            var cases = await query
                .OrderByDescending(c => c.CreatedOn)
                .ToListAsync(ct);

            var rows = cases.Select(c =>
            {
                var openFindings = c.ComplianceFindings
                    .Where(f => !f.IsResolved)
                    .ToList();

                var lastLetter = c.CaseLetters
                    .OrderByDescending(l => l.SentOn)
                    .FirstOrDefault();

                var startedOn = c.AssignedOn ?? c.CreatedOn;
                var daysOpen = (now - startedOn).Days;

                // Correctly handle the non-nullable ResponseDueOn on the entity.
                DateTime? responseDue = lastLetter != null
                    ? lastLetter.ResponseDueOn
                    : (DateTime?)null;

                int? daysUntilDue = responseDue.HasValue
                    ? (int)(responseDue.Value - now).TotalDays
                    : (int?)null;

                DateTime? lastLetterSentOn = lastLetter != null
                    ? lastLetter.SentOn
                    : (DateTime?)null;

                return new NonCompliantCaseReportRow
                {
                    CaseId = c.Id,
                    TenderNumber = c.Tender.TenderNumber,
                    TenderTitle = c.Tender.Title,
                    EmployerName = c.Tender.EmployerName,
                    Province = c.Tender.Province,

                    AgentId = c.AgentId,
                    AgentName = c.Agent?.FullName,

                    Status = c.Status,
                    Priority = c.Priority,

                    FindingsCount = openFindings.Count,
                    FindingStreams = string.Join(", ",
                        openFindings.Select(f => f.Stream.ToString()).Distinct()),

                    CreatedOn = c.CreatedOn,
                    AssignedOn = c.AssignedOn,
                    DaysOpen = daysOpen,

                    IsPastSla = daysOpen > sla,
                    IsSlaWarning = daysOpen > sla - warning && daysOpen <= sla,

                    LastLetterType = lastLetter?.Type.ToString(),
                    LastLetterSentOn = lastLetterSentOn,
                    ResponseDueOn = responseDue,
                    DaysUntilDue = daysUntilDue
                };
            })
            .OrderByDescending(r => r.DaysOpen)
            .ToList();

            return rows;
        }
    }
}