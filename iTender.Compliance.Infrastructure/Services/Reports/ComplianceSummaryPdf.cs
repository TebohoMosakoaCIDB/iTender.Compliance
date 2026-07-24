using iTender.Compliance.Application.DTOs.Reports;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace iTender.Compliance.Infrastructure.Services.Reports
{
    public class ComplianceSummaryPdf : IDocument
    {
        private readonly ComplianceSummaryReportModel _model;

        public ComplianceSummaryPdf(
            ComplianceSummaryReportModel model)
        {
            _model = model;
        }

        public DocumentMetadata GetMetadata() =>
            DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(30);

                page.Header()
                    .Text("Compliance Summary Report")
                    .FontSize(22)
                    .Bold();

                page.Content()
                    .PaddingVertical(20)
                    .Column(column =>
                    {
                        column.Spacing(10);

                        column.Item().Text($"Generated: {DateTime.Now:dd MMM yyyy HH:mm}");

                        column.Item().Text($"Total Cases: {_model.TotalCases}");

                        column.Item().Text($"Assigned Cases: {_model.AssignedCases}");

                        column.Item().Text($"Waiting Response: {_model.WaitingForResponse}");

                        column.Item().Text($"Compliant: {_model.CompliantCases}");

                        column.Item().Text($"Non-Compliant: {_model.NonCompliantCases}");

                        column.Item().Text($"Closed: {_model.ClosedCases}");
                    });

                page.Footer()
                    .AlignCenter()
                    .Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                        x.Span(" of ");
                        x.TotalPages();
                    });
            });
        }
    }
}
