using iTender.Compliance.Application.DTOs.Reports;
using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Application.Interfaces.Services;
using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Infrastructure.Services;

public class ReportService : IReportService
{
    private readonly IComplianceCaseRepository _complianceCaseRepository;
    private readonly ITenderSyncRepository _tenderSyncRepository;
    private readonly ICaseLetterRepository _caseLetterRepository;
    private readonly IAuditLogRepository _auditLogRepository;

    public ReportService(
        IComplianceCaseRepository complianceCaseRepository,
        ITenderSyncRepository tenderSyncRepository,
        ICaseLetterRepository caseLetterRepository,
        IAuditLogRepository auditLogRepository)
    {
        _complianceCaseRepository = complianceCaseRepository;
        _tenderSyncRepository = tenderSyncRepository;
        _caseLetterRepository = caseLetterRepository;
        _auditLogRepository = auditLogRepository;
    }

    public async Task<ComplianceSummaryReportModel> GetComplianceSummaryAsync(
        CancellationToken cancellationToken = default)
    {
        var cases = await _complianceCaseRepository.GetAllAsync(cancellationToken);

        return new ComplianceSummaryReportModel
        {
            TotalCases = cases.Count,

            AssignedCases = cases.Count(x => x.Status == CaseStatus.Assigned),

            WaitingForResponse = cases.Count(x => x.Status == CaseStatus.WaitingForResponse),

            CompliantCases = cases.Count(x => x.Outcome == ComplianceOutcome.Compliant),

            NonCompliantCases = cases.Count(x => x.Outcome == ComplianceOutcome.NonCompliant),

            ClosedCases = cases.Count(x => x.Status == CaseStatus.Closed)
        };
    }

    public async Task<List<OutstandingCasesReportModel>> GetOutstandingCasesAsync(
        CancellationToken cancellationToken = default)
    {
        var cases = await _complianceCaseRepository.GetAllAsync(cancellationToken);

        return cases
            .Where(x =>
                x.Status != CaseStatus.Closed)
            .OrderBy(x => x.Tender!.ClosingDate)
            .Select(x => new OutstandingCasesReportModel
            {
                TenderNumber = x.Tender!.TenderNumber,
                TenderTitle = x.Tender.Title,
                Employer = x.Tender.EmployerName,
                AssignedAgent = x.Agent?.FullName ?? "Unassigned",
                Status = x.Status,
                Priority = x.Priority
            })
            .ToList();
    }

    public async Task<List<AgentPerformanceReportModel>> GetAgentPerformanceAsync(
        CancellationToken cancellationToken = default)
    {
        var cases = await _complianceCaseRepository.GetAllAsync(cancellationToken);

        return cases
            .Where(x => x.Agent != null)
            .GroupBy(x => x.Agent!.FullName)
            .Select(g => new AgentPerformanceReportModel
            {
                AgentName = g.Key,
                AssignedCases = g.Count(),
                CompletedCases = g.Count(x => x.Status == CaseStatus.Closed),
                PendingCases = g.Count(x => x.Status != CaseStatus.Closed)
            })
            .OrderByDescending(x => x.AssignedCases)
            .ToList();
    }

    public async Task<List<SynchronizationReportModel>> GetSynchronizationHistoryAsync(
        CancellationToken cancellationToken = default)
    {
        var syncs = await _tenderSyncRepository.GetAllAsync(cancellationToken);

        return syncs
            .OrderByDescending(x => x.StartedOn)
            .Select(x => new SynchronizationReportModel
            {
                StartedOn = x.StartedOn,
                CompletedOn = x.CompletedOn,
                Status = x.Status,
                TotalRetrieved = x.TotalRetrieved,
                CasesCreated = x.CasesCreated,
                ErrorCount = x.ErrorCount
            })
            .ToList();
    }

    public async Task<List<LetterHistoryReportModel>> GetLetterHistoryAsync(
        CancellationToken cancellationToken = default)
    {
        var letters = await _caseLetterRepository.GetOutstandingAsync(cancellationToken);

        return letters
            .OrderByDescending(x => x.SentOn)
            .Select(x => new LetterHistoryReportModel
            {
                TenderNumber = x.ComplianceCase!.Tender!.TenderNumber,
                RecipientName = x.RecipientName,
                LetterType = x.Type,
                SentOn = x.SentOn,
                RespondedOn = x.RespondedOn
            })
            .ToList();
    }

    public async Task<List<AuditReportModel>> GetAuditHistoryAsync(
        CancellationToken cancellationToken = default)
    {
        var audits = await _auditLogRepository.GetAllAsync(cancellationToken);

        return audits
            .OrderByDescending(x => x.CreatedOn)
            .Select(x => new AuditReportModel
            {
                Date = x.CreatedOn,
                Action = x.Action,
                Entity = x.Entity,
                Description = x.Description
            })
            .ToList();
    }
}