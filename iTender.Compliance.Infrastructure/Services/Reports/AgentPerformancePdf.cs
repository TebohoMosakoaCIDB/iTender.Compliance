using iTender.Compliance.Application.DTOs.Reports;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;

namespace iTender.Compliance.Infrastructure.Services.Reports
{
    public class AgentPerformancePdf : IDocument
    {
        private readonly List<AgentPerformanceReportModel> _model;

        public AgentPerformancePdf(List<AgentPerformanceReportModel> model)
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
                    .Text("Agent Performance")
                    .FontSize(20)
                    .Bold();

                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(2);
                        c.RelativeColumn();
                        c.RelativeColumn();
                        c.RelativeColumn();
                    });

                    table.Header(h =>
                    {
                        h.Cell().Text("Agent");
                        h.Cell().Text("Assigned");
                        h.Cell().Text("Completed");
                        h.Cell().Text("Pending");
                    });

                    foreach (var item in _model)
                    {
                        table.Cell().Text(item.AgentName);
                        table.Cell().Text(item.AssignedCases.ToString());
                        table.Cell().Text(item.CompletedCases.ToString());
                        table.Cell().Text(item.PendingCases.ToString());
                    }
                });
            });
        }
    }
}
