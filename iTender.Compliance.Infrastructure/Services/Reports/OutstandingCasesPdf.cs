using iTender.Compliance.Application.DTOs.Reports;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;

namespace iTender.Compliance.Infrastructure.Services.Reports
{
    public class OutstandingCasesPdf : IDocument
    {
        private readonly List<OutstandingCasesReportModel> _model;

        public OutstandingCasesPdf(List<OutstandingCasesReportModel> model)
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
                    .Text("Outstanding Compliance Cases")
                    .FontSize(20)
                    .Bold();

                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn();
                        columns.RelativeColumn(2);
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                    });

                    table.Header(header =>
                    {
                        header.Cell().Text("Tender");
                        header.Cell().Text("Title");
                        header.Cell().Text("Agent");
                        header.Cell().Text("Status");
                        header.Cell().Text("Priority");
                    });

                    foreach (var item in _model)
                    {
                        table.Cell().Text(item.TenderNumber);
                        table.Cell().Text(item.TenderTitle);
                        table.Cell().Text(item.AssignedAgent);
                        table.Cell().Text(item.Status);
                        table.Cell().Text(item.Priority);
                    }
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.CurrentPageNumber();
                    x.Span(" / ");
                    x.TotalPages();
                });
            });
        }
    }
}
