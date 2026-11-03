using iTender.Compliance.Application.DTOs;
using iTender.Compliance.Application.Interfaces.Services;
using iTender.Compliance.Application.Models.Reports;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace iTender.Compliance.Infrastructure.Services
{
    public class NonCompliantCasesPdfService : INonCompliantCasesPdfService
    {
        private readonly IReportingService _reportingService;

        public NonCompliantCasesPdfService(IReportingService reportingService)
        {
            _reportingService = reportingService;
        }

        public async Task<byte[]> GenerateAsync(
            NonCompliantCaseReportFilter filter,
            CancellationToken ct = default)
        {
            var rows = await _reportingService
                .GetNonCompliantCasesReportAsync(filter, ct);

            return BuildPdf(rows, filter);
        }

        private static byte[] BuildPdf(
            IReadOnlyList<NonCompliantCaseReportRow> rows,
            NonCompliantCaseReportFilter filter)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(8));

                    page.Header().Element(x => Header(x, filter));
                    page.Content().Element(x => Content(x, rows));
                    page.Footer().Element(Footer);
                });
            });

            return document.GeneratePdf();
        }

        private static void Header(
            IContainer container,
            NonCompliantCaseReportFilter filter)
        {
            container.Column(column =>
            {
                column.Item().Row(row =>
                {
                    row.RelativeItem().Column(left =>
                    {
                        left.Item().Height(45).Image("wwwroot/cidb-logo.png");
                        left.Item()
                            .PaddingTop(3)
                            .Text("CIDB Regulatory Compliance Intelligence Platform (CRCIP)")
                            .FontSize(10)
                            .FontColor("#666666");
                    });

                    row.RelativeItem().AlignRight().Column(right =>
                    {
                        right.Item()
                            .Text("NON-COMPLIANT CASES")
                            .Bold()
                            .FontSize(16);

                        right.Item()
                            .Text($"Generated {DateTime.UtcNow:yyyy-MM-dd HH:mm}")
                            .FontSize(9)
                            .FontColor("#666666");
                    });
                });

                var filters = new List<string>();
                if (!string.IsNullOrWhiteSpace(filter.EmployerName))
                    filters.Add($"Employer: {filter.EmployerName}");
                if (!string.IsNullOrWhiteSpace(filter.Province))
                    filters.Add($"Province: {filter.Province}");
                if (filter.Status.HasValue)
                    filters.Add($"Status: {filter.Status}");
                if (filter.FromDate.HasValue)
                    filters.Add($"From: {filter.FromDate:yyyy-MM-dd}");
                if (filter.ToDate.HasValue)
                    filters.Add($"To: {filter.ToDate:yyyy-MM-dd}");

                if (filters.Any())
                {
                    column.Item()
                        .PaddingTop(5)
                        .Text(string.Join("  ·  ", filters))
                        .FontSize(8)
                        .FontColor("#666666");
                }

                column.Item()
                    .PaddingTop(8)
                    .LineHorizontal(1)
                    .LineColor("#B3202A");
            });
        }

        private static void Content(
            IContainer container,
            IReadOnlyList<NonCompliantCaseReportRow> rows)
        {
            container.Column(column =>
            {
                if (!rows.Any())
                {
                    column.Item()
                        .PaddingTop(15)
                        .Text("No non-compliant cases match the filters.")
                        .FontColor("#666666");
                    return;
                }

                column.Item()
                    .PaddingTop(10)
                    .Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1.6f);   // Tender
                            columns.RelativeColumn(2.4f);   // Title
                            columns.RelativeColumn(1.8f);   // Employer
                            columns.RelativeColumn(1.4f);   // Officer
                            columns.RelativeColumn(1.2f);   // Status
                            columns.RelativeColumn(0.8f);   // Findings
                            columns.RelativeColumn(0.8f);   // Days open
                            columns.RelativeColumn(0.9f);   // SLA
                            columns.RelativeColumn(1.4f);   // Last letter
                            columns.RelativeColumn(1.2f);   // Response due
                        });

                        HeaderRow(table);

                        foreach (var r in rows)
                        {
                            Cell(table, r.TenderNumber, false);
                            Cell(table, Truncate(r.TenderTitle, 60), false);
                            Cell(table, Truncate(r.EmployerName, 40), false);
                            Cell(table, r.AgentName ?? "-", false);
                            Cell(table, r.Status.ToString(), false);
                            Cell(table, r.FindingsCount.ToString(), true);
                            Cell(table, r.DaysOpen.ToString(), true);

                            string sla;
                            if (r.IsPastSla) sla = "Breached";
                            else if (r.IsSlaWarning) sla = "Warning";
                            else sla = "On Track";
                            Cell(table, sla, false);

                            var letter = string.IsNullOrWhiteSpace(r.LastLetterType)
                                ? "-"
                                : $"{r.LastLetterType} {r.LastLetterSentOn:dd MMM}";
                            Cell(table, letter, false);

                            string due;
                            if (r.ResponseDueOn.HasValue)
                            {
                                due = r.DaysUntilDue < 0
                                    ? $"{Math.Abs(r.DaysUntilDue!.Value)}d overdue"
                                    : $"{r.ResponseDueOn:dd MMM} ({r.DaysUntilDue}d)";
                            }
                            else
                            {
                                due = "-";
                            }
                            Cell(table, due, false);
                        }
                    });

                column.Item()
                    .PaddingTop(10)
                    .Text($"{rows.Count} case(s) in scope.")
                    .FontSize(8)
                    .FontColor("#666666");
            });
        }

        private static void HeaderRow(TableDescriptor table)
        {
            var headers = new[]
            {
                "Tender", "Title", "Employer", "Officer", "Status",
                "Findings", "Days Open", "SLA", "Last Letter", "Response Due"
            };

            foreach (var h in headers)
            {
                table.Cell()
                    .Background("#B3202A")
                    .Padding(5)
                    .Text(h)
                    .FontColor("#FFFFFF")
                    .Bold()
                    .FontSize(7);
            }
        }

        private static void Cell(TableDescriptor table, string value, bool alignRight)
        {
            var cell = table.Cell()
                .BorderBottom(1)
                .BorderColor("#DDDDDD")
                .Padding(4);

            if (alignRight)
                cell.AlignRight().Text(value).FontSize(7);
            else
                cell.Text(value).FontSize(7);
        }

        private static string Truncate(string? value, int max)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            return value.Length <= max
                ? value
                : value[..max] + "…";
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