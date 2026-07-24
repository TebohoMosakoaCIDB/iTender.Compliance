using iTender.Compliance.Application.DTOs.Reports;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace iTender.Compliance.Infrastructure.Services.Reports
{
    public class AuditHistoryPdf : IDocument
    {
        private readonly List<AuditReportModel> _model;

        public AuditHistoryPdf(List<AuditReportModel> model)
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
                    .Text("Audit History")
                    .FontSize(20)
                    .Bold();

                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn();
                        c.RelativeColumn();
                        c.RelativeColumn();
                        c.RelativeColumn(3);
                    });

                    table.Header(h =>
                    {
                        h.Cell().Text("Date");
                        h.Cell().Text("User");
                        h.Cell().Text("Action");
                        h.Cell().Text("Description");
                    });

                    foreach (var item in _model)
                    {
                        table.Cell().Text(item.Date.ToString("dd MMM yyyy HH:mm"));
                        table.Cell().Text(item.User);
                        table.Cell().Text(item.Action);
                        table.Cell().Text(item.Description);
                    }
                });
            });
        }
    }
}
