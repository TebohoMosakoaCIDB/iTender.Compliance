using iTender.Compliance.Application.DTOs.Reports;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace iTender.Compliance.Infrastructure.Services.Reports
{
    public class SynchronizationHistoryPdf : IDocument
    {
        private readonly List<SynchronizationReportModel> _model;

        public SynchronizationHistoryPdf(List<SynchronizationReportModel> model)
        {
            _model = model;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(25);

                page.Header()
                    .Text("Synchronization History")
                    .FontSize(20)
                    .Bold();

                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn();
                        c.RelativeColumn();
                        c.RelativeColumn();
                        c.RelativeColumn();
                        c.RelativeColumn();
                    });

                    table.Header(h =>
                    {
                        h.Cell().Text("Started");
                        h.Cell().Text("Status");
                        h.Cell().Text("Retrieved");
                        h.Cell().Text("Cases");
                        h.Cell().Text("Errors");
                    });

                    foreach (var item in _model)
                    {
                        table.Cell().Text(item.StartedOn.ToString("dd MMM yyyy"));
                        table.Cell().Text(item.Status);
                        table.Cell().Text(item.TotalRetrieved.ToString());
                        table.Cell().Text(item.CasesCreated.ToString());
                        table.Cell().Text(item.ErrorCount.ToString());
                    }
                });
            });
        }
    }
}
