using iTender.Compliance.Application.Interfaces.Services;
using iTender.Compliance.Infrastructure.Services.Reports;
using QuestPDF.Fluent;

namespace iTender.Compliance.Infrastructure.Services
{
    public class PdfReportService : IPdfReportService
    {
        private readonly IReportService _reportService;

        public PdfReportService(
            IReportService reportService)
        {
            _reportService = reportService;
        }

        public async Task<byte[]> GenerateComplianceSummaryAsync(
            CancellationToken cancellationToken = default)
        {
            var model = await _reportService.GetComplianceSummaryAsync(cancellationToken);

            var document = new ComplianceSummaryPdf(model);

            return document.GeneratePdf();
        }

        public async Task<byte[]> GenerateOutstandingCasesAsync(
            CancellationToken cancellationToken = default)
        {
            var model = await _reportService.GetOutstandingCasesAsync(cancellationToken);

            var document = new OutstandingCasesPdf(model);

            return document.GeneratePdf();
        }

        public async Task<byte[]> GenerateAgentPerformanceAsync(
            CancellationToken cancellationToken = default)
        {
            var model = await _reportService.GetAgentPerformanceAsync(cancellationToken);

            var document = new AgentPerformancePdf(model);

            return document.GeneratePdf();
        }

        public async Task<byte[]> GenerateSynchronizationHistoryAsync(
            CancellationToken cancellationToken = default)
        {
            var model = await _reportService.GetSynchronizationHistoryAsync(cancellationToken);

            var document = new SynchronizationHistoryPdf(model);

            return document.GeneratePdf();
        }

        public async Task<byte[]> GenerateLetterHistoryAsync(
            CancellationToken cancellationToken = default)
        {
            var model = await _reportService.GetLetterHistoryAsync(cancellationToken);

            var document = new LetterHistoryPdf(model);

            return document.GeneratePdf();
        }

        public async Task<byte[]> GenerateAuditHistoryAsync(
            CancellationToken cancellationToken = default)
        {
            var model = await _reportService.GetAuditHistoryAsync(cancellationToken);

            var document = new AuditHistoryPdf(model);

            return document.GeneratePdf();
        }
    }
}
