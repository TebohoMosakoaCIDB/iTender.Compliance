using iTender.Compliance.Application.DTOs;
using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Application.Interfaces.Services;
using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Infrastructure.Services;

public class ReportService : IReportService
{
    private readonly IReportRepository _reportRepository;

    public ReportService(IReportRepository reportRepository)
    {
        _reportRepository = reportRepository;
    }

    public async Task<ReportSummaryModel> GetReportAsync(
    DateTime fromDate,
    DateTime toDate)
    {
        fromDate = DateTime.SpecifyKind(fromDate, DateTimeKind.Utc);
        toDate = DateTime.SpecifyKind(toDate, DateTimeKind.Utc);

        var totalTenders =
            await _reportRepository.GetTotalTendersAsync(
                fromDate,
                toDate);

        var totalCases =
            await _reportRepository.GetTotalCasesAsync(
                fromDate,
                toDate);

        var cases =
            await _reportRepository.GetCasesAsync(
                fromDate,
                toDate);

        var statusBreakdown =
            await _reportRepository.GetStatusBreakdownAsync(
                fromDate,
                toDate);

        var outcomeBreakdown =
            await _reportRepository.GetOutcomeBreakdownAsync(
                fromDate,
                toDate);

        var compliant =
            cases.Count(x =>
                x.Outcome == ComplianceOutcome.Compliant);

        var nonCompliant =
            cases.Count(x =>
                x.Outcome == ComplianceOutcome.NonCompliant);

        var closed =
            cases.Count(x =>
                x.Status == CaseStatus.Closed);

        var escalated =
            cases.Count(x =>
                x.Status == CaseStatus.Escalated);

        var inProgress = cases.Count - closed;

        var complianceRate =
            totalCases == 0
                ? 0
                : Math.Round(
                    (decimal)compliant / totalCases * 100,
                    2);

        return new ReportSummaryModel
        {
            TotalTenders = totalTenders,
            TotalCases = totalCases,
            CasesOpened = totalCases,
            CasesClosed = closed,
            CasesInProgress = inProgress,
            CasesEscalated = escalated,
            Compliant = compliant,
            NonCompliant = nonCompliant,
            ComplianceRate = complianceRate,
            StatusBreakdown = statusBreakdown,
            OutcomeBreakdown = outcomeBreakdown
        };
    }

    public async Task<List<ReportTenderModel>> GetNonCompliantTendersAsync(
    DateTime fromDate,
    DateTime toDate)
    {
        fromDate = DateTime.SpecifyKind(fromDate, DateTimeKind.Utc);
        toDate = DateTime.SpecifyKind(toDate, DateTimeKind.Utc);

        return await _reportRepository
            .GetNonCompliantTendersAsync(
                fromDate,
                toDate);
    }
}