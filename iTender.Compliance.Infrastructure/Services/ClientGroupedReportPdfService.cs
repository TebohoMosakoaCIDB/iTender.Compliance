using iTender.Compliance.Application.DTOs;
using iTender.Compliance.Application.Interfaces.Services;
using iTender.Compliance.Application.Models.Reports;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace iTender.Compliance.Infrastructure.Services
{
    public class ClientGroupedReportPdfService : IClientGroupedReportPdfService
    {
        private readonly IReportingService _reportingService;

        public ClientGroupedReportPdfService(IReportingService reportingService)
        {
            _reportingService = reportingService;
        }

        public async Task<byte[]> GenerateAsync(
            Guid? agentId,
            CancellationToken ct = default)
        {
            var rows = await _reportingService.GetClientGroupedReportAsync(agentId, ct);
            return BuildPdf(rows);
        }

        private static byte[] BuildPdf(IReadOnlyList<ClientGroupedReportRow> rows)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(9));

                    page.Header().Element(x => Header(x));
                    page.Content().Element(x => Content(x, rows));
                    page.Footer().Element(Footer);
                });
            });

            return document.GeneratePdf();
        }

        private static void Header(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().Row(row =>
                {
                    row.RelativeItem().Column(left =>
                    {
                        left.Item()
                            .Height(45)
                            .Image("wwwroot/cidb-logo.png");

                        left.Item()
                            .PaddingTop(3)
                            .Text("CIDB Regulatory Compliance Intelligence Platform (CRCIP)")
                            .FontSize(10)
                            .FontColor("#666666");
                    });

                    row.RelativeItem().AlignRight().Column(right =>
                    {
                        right.Item()
                            .Text("CLIENT GROUPED REPORT")
                            .Bold()
                            .FontSize(16);

                        right.Item()
                            .Text($"Generated {DateTime.UtcNow:yyyy-MM-dd HH:mm}")
                            .FontSize(9)
                            .FontColor("#666666");
                    });
                });

                column.Item()
                    .PaddingTop(8)
                    .LineHorizontal(1)
                    .LineColor("#B3202A");
            });
        }

        private static void Content(
            IContainer container,
            IReadOnlyList<ClientGroupedReportRow> rows)
        {
            container.Column(column =>
            {
                if (!rows.Any())
                {
                    column.Item()
                        .PaddingTop(15)
                        .Text("No data available for the current scope.")
                        .FontColor("#666666");
                    return;
                }

                column.Item()
                    .PaddingTop(10)
                    .Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2.4f);   // Employer
                            columns.RelativeColumn(0.9f);   // Tenders
                            columns.RelativeColumn(0.9f);   // Compliant
                            columns.RelativeColumn(0.9f);   // Non-compliant
                            columns.RelativeColumn(0.9f);   // Open
                            columns.RelativeColumn(0.9f);   // Letters
                            columns.RelativeColumn(1.4f);   // Last correspondence
                            columns.RelativeColumn(1.2f);   // Group status
                            columns.RelativeColumn(1.6f);   // Officer
                        });

                        HeaderRow(table);

                        foreach (var r in rows)
                        {
                            Cell(table, r.EmployerName, false);
                            Cell(table, r.TotalTenders.ToString(), true);
                            Cell(table, r.CompliantTenders.ToString(), true);
                            Cell(table, r.NonCompliantTenders.ToString(), true);
                            Cell(table, r.OpenTenders.ToString(), true);
                            Cell(table, r.LettersIssued.ToString(), true);
                            Cell(table, r.LastCorrespondenceDate?.ToString("yyyy-MM-dd") ?? "-", false);
                            Cell(table, r.GroupStatus?.ToString() ?? "-", false);
                            Cell(table, r.AgentName ?? "-", false);
                        }
                    });

                column.Item()
                    .PaddingTop(10)
                    .Text($"{rows.Count} employer(s) in scope.")
                    .FontSize(8)
                    .FontColor("#666666");
            });
        }

        private static void HeaderRow(TableDescriptor table)
        {
            var headers = new[]
            {
                "Employer", "Tenders", "Compliant", "Non-Compliant",
                "Open", "Letters", "Last Correspondence",
                "Group Status", "Officer"
            };

            foreach (var h in headers)
            {
                table.Cell()
                    .Background("#B3202A")
                    .Padding(5)
                    .Text(h)
                    .FontColor("#FFFFFF")
                    .Bold()
                    .FontSize(8);
            }
        }

        private static void Cell(TableDescriptor table, string value, bool alignRight)
        {
            var cell = table.Cell()
                .BorderBottom(1)
                .BorderColor("#DDDDDD")
                .Padding(4);

            if (alignRight)
                cell.AlignRight().Text(value).FontSize(8);
            else
                cell.Text(value).FontSize(8);
        }

        private static void Footer(IContainer container)
        {
            container
                .BorderTop(1)
                .BorderColor("#DDDDDD")
                .PaddingTop(5)
                .Row(row =>
                {
                    row.RelativeItem()
                        .Text("CRCIP Compliance")
                        .FontSize(8)
                        .FontColor("#666666");

                    row.RelativeItem()
                        .AlignRight()
                        .Text(text =>
                        {
                            text.Span("Page ").FontSize(8);
                            text.CurrentPageNumber().FontSize(8);
                            text.Span(" of ").FontSize(8);
                            text.TotalPages().FontSize(8);
                        });
                });
        }
    }
}