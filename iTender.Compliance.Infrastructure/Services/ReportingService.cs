using iTender.Compliance.Application.DTOs;
using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Application.Interfaces.Services;
using iTender.Compliance.Application.Models.Reports;
using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Infrastructure.Services
{
    public class ReportingService : IReportingService
    {
        private readonly IComplianceCaseRepository _complianceCaseRepository;
        private readonly ITenderRepository _tenderRepository;
        private readonly ITenderSyncRepository _tenderSyncRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly ISystemSettingRepository _settingRepository;
        private readonly IAgentRepository _agentRepository;

        public ReportingService(
            IComplianceCaseRepository complianceCaseRepository,
            ITenderSyncRepository tenderSyncRepository,
            ISystemSettingRepository settingRepository,
            IAuditLogRepository auditLogRepository,
            ITenderRepository tenderRepository,
            IAgentRepository agentRepository)
        {
            _complianceCaseRepository = complianceCaseRepository;
            _tenderSyncRepository = tenderSyncRepository;
            _auditLogRepository = auditLogRepository;
            _settingRepository = settingRepository;
            _agentRepository = agentRepository;
            _tenderRepository = tenderRepository;
        }

        // ---------------------------------------------------------------------
        // COMPLIANCE ANALYSIS
        // ---------------------------------------------------------------------

        public Task<IReadOnlyList<ClientGroupedReportRow>> GetClientGroupedReportAsync(
    Guid? agentId,
    CancellationToken ct = default)
    => _complianceCaseRepository.GetClientGroupedReportAsync(agentId, ct);

        public async Task<IReadOnlyList<NonCompliantCaseReportRow>> GetNonCompliantCasesReportAsync(
            NonCompliantCaseReportFilter filter,
            CancellationToken ct = default)
        {
            var settings = await _settingRepository.GetAsync(ct);

            return await _complianceCaseRepository.GetNonCompliantCasesReportAsync(
                filter,
                settings?.AgsaReferralDeadlineDays,
                settings?.SlaWarningDays,
                ct);
        }

        public async Task<ComplianceAnalysisModel> GetComplianceAnalysisAsync(
            DateTime from,
            DateTime to,
            CancellationToken cancellationToken = default)
        {
            var cases = await _complianceCaseRepository.GetForAnalysisAsync(
                from, to, cancellationToken);

            var total = cases.Count;

            var compliant = cases.Count(c => c.Outcome == ComplianceOutcome.Compliant);
            var nonCompliant = cases.Count(c => c.Outcome == ComplianceOutcome.NonCompliant);
            var open = cases.Count(c => c.Status != CaseStatus.Closed);

            var model = new ComplianceAnalysisModel
            {
                FromDate = from,
                ToDate = to,
                TotalCases = total,
                TotalCompliant = compliant,
                TotalNonCompliant = nonCompliant,
                TotalOpen = open
            };

            // ---- By Status ----
            model.ByStatus = cases
                .GroupBy(c => c.Status)
                .Select(g => new AnalysisByStatus
                {
                    Status = g.Key.ToString(),
                    Count = g.Count(),
                    Percentage = total == 0 ? 0 : Math.Round((decimal)g.Count() / total * 100, 1)
                })
                .OrderByDescending(x => x.Count)
                .ToList();

            // ---- By Outcome ----
            model.ByOutcome = cases
                .GroupBy(c => c.Outcome)
                .Select(g => new AnalysisByOutcome
                {
                    Outcome = g.Key.HasValue ? g.Key.Value.ToString() : "Pending",
                    Count = g.Count(),
                    Percentage = total == 0 ? 0 : Math.Round((decimal)g.Count() / total * 100, 1)
                })
                .OrderByDescending(x => x.Count)
                .ToList();

            // ---- By Officer ----
            model.ByOfficer = cases
                .Where(c => c.AgentId.HasValue && c.Agent != null)
                .GroupBy(c => c.Agent!.FullName)
                .Select(g =>
                {
                    var groupTotal = g.Count();
                    var resolved = g.Count(c => c.Status == CaseStatus.Closed);

                    return new AnalysisByOfficer
                    {
                        OfficerName = g.Key,
                        TotalCases = groupTotal,
                        Resolved = resolved,
                        Outstanding = groupTotal - resolved,
                        ResolvedPercentage = groupTotal == 0
                            ? 0
                            : Math.Round((decimal)resolved / groupTotal * 100, 1)
                    };
                })
                .OrderByDescending(x => x.TotalCases)
                .ToList();

            // ---- By Province ----
            model.ByProvince = cases
                .Where(c => c.Tender != null)
                .GroupBy(c => string.IsNullOrWhiteSpace(c.Tender!.Province)
                    ? "Not specified"
                    : c.Tender.Province!)
                .Select(g => new AnalysisByProvince
                {
                    Province = g.Key,
                    TotalCases = g.Count(),
                    Compliant = g.Count(c => c.Outcome == ComplianceOutcome.Compliant),
                    NonCompliant = g.Count(c => c.Outcome == ComplianceOutcome.NonCompliant),
                    Open = g.Count(c => c.Status != CaseStatus.Closed)
                })
                .OrderByDescending(x => x.TotalCases)
                .ToList();

            // ---- By Employer (top 20) ----
            model.ByEmployer = cases
                .Where(c => c.Tender != null && !string.IsNullOrWhiteSpace(c.Tender!.EmployerName))
                .GroupBy(c => c.Tender!.EmployerName)
                .Select(g =>
                {
                    var groupTotal = g.Count();
                    var nonCompliantCount = g.Count(c => c.Outcome == ComplianceOutcome.NonCompliant);

                    return new AnalysisByEmployer
                    {
                        EmployerName = g.Key,
                        TotalCases = groupTotal,
                        Compliant = g.Count(c => c.Outcome == ComplianceOutcome.Compliant),
                        NonCompliant = nonCompliantCount,
                        Open = g.Count(c => c.Status != CaseStatus.Closed),
                        NonComplianceRate = groupTotal == 0
                            ? 0
                            : Math.Round((decimal)nonCompliantCount / groupTotal * 100, 1)
                    };
                })
                .OrderByDescending(x => x.TotalCases)
                .Take(20)
                .ToList();

            // ---- By Month ----
            model.ByMonth = cases
                .GroupBy(c => new { c.CreatedOn.Year, c.CreatedOn.Month })
                .OrderBy(g => g.Key.Year)
                .ThenBy(g => g.Key.Month)
                .Select(g => new AnalysisByMonth
                {
                    Month = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                    Opened = g.Count(),
                    Closed = g.Count(c => c.Status == CaseStatus.Closed),
                    Compliant = g.Count(c => c.Outcome == ComplianceOutcome.Compliant),
                    NonCompliant = g.Count(c => c.Outcome == ComplianceOutcome.NonCompliant)
                })
                .ToList();

            return model;
        }

        // ---------------------------------------------------------------------
        // MONTHLY REPORT
        // ---------------------------------------------------------------------

        public async Task<MonthlyReportModel> GetMonthlyReportAsync(
            int year,
            int month,
            CancellationToken cancellationToken = default)
        {
            var periodStart = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
            var periodEnd = periodStart.AddMonths(1).AddTicks(-1);

            var report = await BuildPeriodReportAsync(
                periodStart, periodEnd, cancellationToken);

            report.PeriodLabel = periodStart.ToString("MMMM yyyy");

            return report;
        }

        // ---------------------------------------------------------------------
        // QUARTERLY REPORT
        // ---------------------------------------------------------------------

        public async Task<MonthlyReportModel> GetQuarterlyReportAsync(
            int year,
            int quarter,
            CancellationToken cancellationToken = default)
        {
            if (quarter < 1 || quarter > 4)
                throw new ArgumentOutOfRangeException(nameof(quarter));

            var startMonth = ((quarter - 1) * 3) + 1;
            var periodStart = new DateTime(year, startMonth, 1, 0, 0, 0, DateTimeKind.Utc);
            var periodEnd = periodStart.AddMonths(3).AddTicks(-1);

            var report = await BuildPeriodReportAsync(
                periodStart, periodEnd, cancellationToken);

            report.PeriodLabel = $"Q{quarter} {year}";

            return report;
        }

        // ---------------------------------------------------------------------
        // SHARED PERIOD REPORT (used by monthly and quarterly)
        // ---------------------------------------------------------------------

        private async Task<MonthlyReportModel> BuildPeriodReportAsync(
            DateTime periodStart,
            DateTime periodEnd,
            CancellationToken cancellationToken)
        {
            // Load all cases for open-state metrics; use case letters for IL/CN detection.
            var allCases = await _complianceCaseRepository.GetAllAsync(cancellationToken);

            // For the period, we need to know findings and letters.
            // GetAllAsync in the current repo doesn't include them, so use the case-detail
            // path or add a specialised method. For now, filter and load per-case.
            var periodCases = new List<Domain.Entities.ComplianceCase>();

            foreach (var c in allCases.Where(x =>
                x.CreatedOn >= periodStart && x.CreatedOn <= periodEnd))
            {
                var detail = await _complianceCaseRepository.GetByIdAsync(c.Id, cancellationToken);
                if (detail is not null)
                    periodCases.Add(detail);
            }

            var tenders = await _tenderRepository.GetAllAsync(cancellationToken);
            var periodTenders = tenders
                .Where(t => t.AdvertisedDate >= periodStart && t.AdvertisedDate <= periodEnd)
                .ToList();

            var settings = await _settingRepository.GetAsync(cancellationToken);
            var slaDays = settings?.AgsaReferralDeadlineDays ?? 30;

            var openCases = allCases
                .Where(c => c.Status != CaseStatus.Closed)
                .ToList();

            var averageAgeDays = openCases.Count == 0
                ? 0m
                : (decimal)openCases.Average(c => (DateTime.UtcNow - c.CreatedOn).TotalDays);

            return new MonthlyReportModel
            {
                Year = periodStart.Year,
                Month = periodStart.Month,
                PeriodLabel = string.Empty,
                PeriodStart = periodStart,
                PeriodEnd = periodEnd,

                ProjectsIdentified = periodTenders.Count(t => t.IsConstruction),
                ProjectsVerified = periodCases.Count(c =>
                    c.ComplianceFindings != null &&
                    c.ComplianceFindings.Any(f => f.IsResolved)),
                ProjectsRegistered = periodTenders.Count(t => t.IsRegisteredOnRoP),
                ProjectsNotRegistered = periodTenders.Count(t => !t.IsRegisteredOnRoP),

                InitiallyNonCompliant = periodCases.Count,

                Resolved = periodCases.Count(c =>
                    c.Outcome == ComplianceOutcome.Compliant),

                ResolvedAfterIL = periodCases.Count(c =>
                    c.Outcome == ComplianceOutcome.Compliant &&
                    c.ClosureReason == CaseClosureReason.ClientComplied &&
                    (c.CaseLetters == null ||
                     !c.CaseLetters.Any(l => l.Type == LetterType.ContraventionNotice))),

                ResolvedAfterCN = periodCases.Count(c =>
                    c.Outcome == ComplianceOutcome.Compliant &&
                    c.ClosureReason == CaseClosureReason.ClientComplied &&
                    c.CaseLetters != null &&
                    c.CaseLetters.Any(l => l.Type == LetterType.ContraventionNotice)),

                RemainingNonCompliant = allCases.Count(c =>
                    c.Status != CaseStatus.Closed &&
                    c.Status != CaseStatus.ReferredForEnforcement &&
                    c.ComplianceFindings != null &&
                    c.ComplianceFindings.Any(f => !f.IsResolved)),

                ReferredToEnforcement = allCases.Count(c =>
                    c.Status == CaseStatus.ReferredForEnforcement),

                EscalatedToAgsa = allCases.Count(c =>
                    c.Status == CaseStatus.ReferredForEnforcement),

                CorrectiveActionsTaken = periodCases.Count(c =>
                    c.ComplianceActions != null &&
                    c.ComplianceActions.Any(a => a.Status == ComplianceActionStatus.Completed)),

                CasesOpened = periodCases.Count,

                CasesClosed = periodCases.Count(c => c.Status == CaseStatus.Closed),

                AverageCaseAgeDays = Math.Round(averageAgeDays, 1),

                CasesPastSlaDeadline = openCases.Count(c =>
                    c.AssignedOn.HasValue &&
                    (DateTime.UtcNow - c.AssignedOn.Value).TotalDays > slaDays)
            };
        }

        // ---------------------------------------------------------------------
        // OTHER REPORTS
        // ---------------------------------------------------------------------

        public async Task<ComplianceReportModel> GetComplianceReportAsync(
            DateTime from,
            DateTime to,
            CancellationToken cancellationToken = default)
        {
            var cases = await _complianceCaseRepository.GetAllAsync(cancellationToken);

            cases = cases
                .Where(x => x.CreatedOn >= from && x.CreatedOn <= to)
                .ToList();

            var compliant = cases.Count(x => x.Outcome == ComplianceOutcome.Compliant);
            var nonCompliant = cases.Count(x => x.Outcome == ComplianceOutcome.NonCompliant);

            return new ComplianceReportModel
            {
                TotalCases = cases.Count,
                CompliantCases = compliant,
                NonCompliantCases = nonCompliant,
                CompliancePercentage = cases.Count == 0 ? 0 : compliant * 100d / cases.Count,
                NonCompliancePercentage = cases.Count == 0 ? 0 : nonCompliant * 100d / cases.Count
            };
        }

        public async Task<List<AgentReportModel>> GetAgentReportAsync(
            CancellationToken cancellationToken = default)
        {
            var agents = await _agentRepository.GetAllAsync(cancellationToken);
            var cases = await _complianceCaseRepository.GetAllAsync(cancellationToken);

            return agents.Select(agent => new AgentReportModel
            {
                AgentName = agent.FullName,
                AssignedCases = cases.Count(x => x.AgentId == agent.Id),
                ClosedCases = cases.Count(x => x.AgentId == agent.Id && x.Status == CaseStatus.Closed),
                OutstandingCases = cases.Count(x => x.AgentId == agent.Id && x.Status != CaseStatus.Closed)
            }).ToList();
        }

        public async Task<SyncReportModel> GetSynchronizationReportAsync(
            DateTime from,
            DateTime to,
            CancellationToken cancellationToken = default)
        {
            var syncs = await _tenderSyncRepository.GetAllAsync(cancellationToken);

            syncs = syncs
                .Where(x => x.StartedOn >= from && x.StartedOn <= to)
                .ToList();

            return new SyncReportModel
            {
                TotalSynchronizations = syncs.Count,
                TotalRetrieved = syncs.Sum(x => x.TotalRetrieved),
                TotalCasesCreated = syncs.Sum(x => x.CasesCreated),
                FailedSynchronizations = syncs.Count(x => x.ErrorCount > 0)
            };
        }

        public async Task<List<AuditReportModel>> GetAuditReportAsync(
            DateTime from,
            DateTime to,
            CancellationToken cancellationToken = default)
        {
            var logs = await _auditLogRepository.GetAllAsync(cancellationToken);

            return logs
                .Where(x => x.CreatedOn >= from && x.CreatedOn <= to)
                .Select(x => new AuditReportModel
                {
                    Date = x.CreatedOn,
                    Action = x.Action.ToString(),
                    Entity = x.Entity.ToString(),
                    Description = x.Description,
                    User = x.CreatedBy?.ToString() ?? "System"
                })
                .ToList();
        }

        public async Task<IReadOnlyList<LateRopRegistrationModel>> GetLateRopRegistrationsAsync(
            DateTime from,
            DateTime to,
            CancellationToken cancellationToken = default)
        {
            var settings = await _settingRepository.GetAsync(cancellationToken);
            var graceDays = settings?.RopRegistrationGraceDays ?? 21;

            var tenders = await _tenderRepository.GetLateRopRegistrationsAsync(
                from, to, graceDays, cancellationToken);

            return tenders.Select(t => new LateRopRegistrationModel
            {
                TenderId = t.Id,
                TenderNumber = t.TenderNumber,
                TenderTitle = t.Title,
                EmployerName = t.EmployerName,
                ContactName = t.ContactName,
                ContactEmail = t.ContactEmail,
                ClosingDate = t.ClosingDate,
                AwardedDate = t.AwardedDate ?? default,
                AwardValue = t.AwardValue,
                RoPRegistrationDate = t.RoPRegistrationDate ?? default,
                RegistrationDeadline = t.ClosingDate.AddDays(graceDays),
                DaysLate = t.RoPRegistrationDate.HasValue
                    ? Math.Max(0, (t.RoPRegistrationDate.Value.Date
                                   - t.ClosingDate.AddDays(graceDays).Date).Days)
                    : 0
            }).ToList();
        }
    }
}