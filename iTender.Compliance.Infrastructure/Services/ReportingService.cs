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
