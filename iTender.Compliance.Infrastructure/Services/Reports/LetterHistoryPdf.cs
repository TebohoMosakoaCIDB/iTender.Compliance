using iTender.Compliance.Application.DTOs.Reports;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace iTender.Compliance.Infrastructure.Services.Reports
{
    public class LetterHistoryPdf : IDocument
    {
        private readonly List<LetterHistoryReportModel> _model;

        public LetterHistoryPdf(List<LetterHistoryReportModel> model)
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
                    .Text("Letter History")
                    .FontSize(20)
                    .Bold();

                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn();
                        c.RelativeColumn(2);
                        c.RelativeColumn();
                        c.RelativeColumn();
                    });

                    table.Header(h =>
                    {
                        h.Cell().Text("Tender");
                        h.Cell().Text("Recipient");
                        h.Cell().Text("Letter");
                        h.Cell().Text("Sent");
                    });

                    foreach (var item in _model)
                    {
                        table.Cell().Text(item.TenderNumber);
                        table.Cell().Text(item.RecipientName);
                        table.Cell().Text(item.LetterType);
                        table.Cell().Text(item.SentOn.ToString("dd MMM yyyy"));
                    }
                });
            });
        }
    }
}
