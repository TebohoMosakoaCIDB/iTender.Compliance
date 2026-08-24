using iTender.Compliance.Application.DTOs;
using iTender.Compliance.Application.Interfaces.Services;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace iTender.Compliance.Infrastructure.Services
{
    public class ReportPdfService : IReportPdfService
    {
        private readonly IReportService _reportService;

        public ReportPdfService(
            IReportService reportService)
        {
            _reportService = reportService;
        }

        public async Task<byte[]> GenerateAsync(
            DateTime fromDate,
            DateTime toDate)
        {
            var summary =
                await _reportService.GetReportAsync(
                    fromDate,
                    toDate);

            var nonCompliantTenders =
                await _reportService.GetNonCompliantTendersAsync(
                    fromDate,
                    toDate);

            var model = new ComplianceReportPdfModel
            {
                FromDate = fromDate,
                ToDate = toDate,
                GeneratedAt = DateTime.UtcNow,
                Summary = summary,
                NonCompliantTenders = nonCompliantTenders
            };

            return GeneratePdf(model);
        }


        private static byte[] GeneratePdf(
            ComplianceReportPdfModel model)
        {
            var document =
                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(35);

                        page.DefaultTextStyle(
                            x => x.FontSize(9));

                        page.Header()
                            .Element(x =>
                                ComposeHeader(x, model));

                        page.Content()
                            .Element(x =>
                                ComposeContent(x, model));

                        page.Footer()
                            .Element(ComposeFooter);
                    });
                });

            return document.GeneratePdf();
        }


        private static void ComposeHeader(
    IContainer container,
    ComplianceReportPdfModel model)
        {
            container.Column(column =>
            {
                column.Item()
                    .Row(row =>
                    {
                        row.RelativeItem()
                            .Column(left =>
                            {
                                left.Item()
                                    .Height(45)
                                    .Image("wwwroot/cidb-logo.png");

                                left.Item()
                                    .PaddingTop(3)
                                    .Text("Compliance Monitoring")
                                    .FontSize(10)
                                    .FontColor("#666666");
                            });

                        row.RelativeItem()
                            .AlignRight()
                            .Column(right =>
                            {
                                right.Item()
                                    .Text("COMPLIANCE REPORT")
                                    .Bold()
                                    .FontSize(16);

                                right.Item()
                                    .Text(
                                        $"{model.FromDate:dd MMM yyyy} - {model.ToDate:dd MMM yyyy}")
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


        private static void ComposeContent(
            IContainer container,
            ComplianceReportPdfModel model)
        {
            container.Column(column =>
            {
                // Report information
                column.Item()
                    .Element(x =>
                        ComposeReportInformation(x, model));

                // Summary
                column.Item()
                    .PaddingTop(15)
                    .Element(x =>
                        ComposeSummary(x, model.Summary));

                // Case activity
                column.Item()
                    .PaddingTop(15)
                    .Element(x =>
                        ComposeCaseActivity(x, model.Summary));

                // Breakdowns
                column.Item()
                    .PaddingTop(15)
                    .Element(x =>
                        ComposeBreakdowns(x, model.Summary));

                // Non-compliant tenders
                column.Item()
                    .PaddingTop(15)
                    .Element(x =>
                        ComposeNonCompliantTenders(
                            x,
                            model.NonCompliantTenders));
            });
        }


        private static void ComposeReportInformation(
            IContainer container,
            ComplianceReportPdfModel model)
        {
            container
                .Background("#F5F5F5")
                .Padding(10)
                .Column(column =>
                {
                    column.Item()
                        .Text("Report Information")
                        .Bold()
                        .FontSize(11);

                    column.Item()
                        .PaddingTop(5)
                        .Row(row =>
                        {
                            row.RelativeItem()
                                .Text(text =>
                                {
                                    text.Span("Period: ")
                                        .Bold();

                                    text.Span(
                                        $"{model.FromDate:yyyy-MM-dd} to {model.ToDate:yyyy-MM-dd}");
                                });

                            row.RelativeItem()
                                .AlignRight()
                                .Text(text =>
                                {
                                    text.Span("Generated: ")
                                        .Bold();

                                    text.Span(
                                        model.GeneratedAt
                                            .ToString(
                                                "yyyy-MM-dd HH:mm"));
                                });
                        });
                });
        }


        private static void ComposeSummary(
            IContainer container,
            ReportSummaryModel summary)
        {
            container.Column(column =>
            {
                column.Item()
                    .Text("Summary")
                    .Bold()
                    .FontSize(13);

                column.Item()
                    .PaddingTop(7)
                    .Row(row =>
                    {
                        SummaryCard(
                            row.RelativeItem(),
                            "Total Tenders",
                            summary.TotalTenders.ToString());

                        SummaryCard(
                            row.RelativeItem(),
                            "Compliance Cases",
                            summary.TotalCases.ToString());

                        SummaryCard(
                            row.RelativeItem(),
                            "Non-Compliant",
                            summary.NonCompliant.ToString());

                        SummaryCard(
                            row.RelativeItem(),
                            "Compliance Rate",
                            $"{summary.ComplianceRate}%");
                    });
            });
        }


        private static void SummaryCard(
            IContainer container,
            string label,
            string value)
        {
            container
                .Border(1)
                .BorderColor("#DDDDDD")
                .Padding(8)
                .Column(column =>
                {
                    column.Item()
                        .Text(label)
                        .FontSize(8)
                        .FontColor("#666666");

                    column.Item()
                        .PaddingTop(3)
                        .Text(value)
                        .Bold()
                        .FontSize(16);
                });
        }


        private static void ComposeCaseActivity(
            IContainer container,
            ReportSummaryModel summary)
        {
            container.Column(column =>
            {
                column.Item()
                    .Text("Case Activity")
                    .Bold()
                    .FontSize(13);

                column.Item()
                    .PaddingTop(7)
                    .Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        TableHeader(
                            table,
                            "Cases Opened",
                            "Cases Closed",
                            "In Progress",
                            "Escalated");

                        table.Cell()
                            .Element(TableCell)
                            .AlignCenter()
                            .Text(summary.CasesOpened.ToString());

                        table.Cell()
                            .Element(TableCell)
                            .AlignCenter()
                            .Text(summary.CasesClosed.ToString());

                        table.Cell()
                            .Element(TableCell)
                            .AlignCenter()
                            .Text(summary.CasesInProgress.ToString());

                        table.Cell()
                            .Element(TableCell)
                            .AlignCenter()
                            .Text(summary.CasesEscalated.ToString());
                    });
            });
        }


        private static void ComposeBreakdowns(
            IContainer container,
            ReportSummaryModel summary)
        {
            container.Row(row =>
            {
                row.RelativeItem()
                    .Element(x =>
                        ComposeStatusBreakdown(
                            x,
                            summary));

                row.ConstantItem(15);

                row.RelativeItem()
                    .Element(x =>
                        ComposeOutcomeBreakdown(
                            x,
                            summary));
            });
        }


        private static void ComposeStatusBreakdown(
            IContainer container,
            ReportSummaryModel summary)
        {
            container.Column(column =>
            {
                column.Item()
                    .Text("Case Status")
                    .Bold()
                    .FontSize(11);

                foreach (var status in summary.StatusBreakdown)
                {
                    column.Item()
                        .PaddingTop(5)
                        .Row(row =>
                        {
                            row.RelativeItem()
                                .Text(status.Status);

                            row.ConstantItem(70)
                                .AlignRight()
                                .Text(
                                    $"{status.Count} ({status.Percentage}%)");
                        });
                }
            });
        }


        private static void ComposeOutcomeBreakdown(
            IContainer container,
            ReportSummaryModel summary)
        {
            container.Column(column =>
            {
                column.Item()
                    .Text("Compliance Outcomes")
                    .Bold()
                    .FontSize(11);

                foreach (var outcome in summary.OutcomeBreakdown)
                {
                    column.Item()
                        .PaddingTop(5)
                        .Row(row =>
                        {
                            row.RelativeItem()
                                .Text(outcome.Outcome);

                            row.ConstantItem(70)
                                .AlignRight()
                                .Text(
                                    $"{outcome.Count} ({outcome.Percentage}%)");
                        });
                }
            });
        }


        private static void ComposeNonCompliantTenders(
            IContainer container,
            List<ReportTenderModel> tenders)
        {
            container.Column(column =>
            {
                column.Item()
                    .Text("Recent Non-Compliant Tenders")
                    .Bold()
                    .FontSize(13);

                if (!tenders.Any())
                {
                    column.Item()
                        .PaddingTop(8)
                        .Text(
                            "No non-compliant tenders found for this period.")
                        .FontColor("#666666");

                    return;
                }

                column.Item()
                    .PaddingTop(7)
                    .Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1.5f);
                            columns.RelativeColumn(2.2f);
                            columns.RelativeColumn(1.2f);
                            columns.RelativeColumn(1f);
                            columns.RelativeColumn(1.5f);
                            columns.RelativeColumn(1.5f);
                        });

                        TableHeader(
                            table,
                            "Tender Number",
                            "Client",
                            "Date",
                            "Priority",
                            "Status",
                            "Outcome");

                        foreach (var tender in tenders)
                        {
                            table.Cell()
                                .Element(TableCell)
                                .Text(tender.TenderNumber ?? "");

                            table.Cell()
                                .Element(TableCell)
                                .Text(tender.ClientName ?? "");

                            table.Cell()
                                .Element(TableCell)
                                .Text(
                                    tender.AdvertisedDate
                                        .ToString("yyyy-MM-dd"));

                            table.Cell()
                                .Element(TableCell)
                                .Text(
                                    tender.Priority.ToString());

                            table.Cell()
                                .Element(TableCell)
                                .Text(
                                    tender.Status.ToString());

                            table.Cell()
                                .Element(TableCell)
                                .Text(
                                    tender.Outcome?.ToString() ?? "");
                        }
                    });
            });
        }


        private static void TableHeader(
            TableDescriptor table,
            params string[] headers)
        {
            foreach (var header in headers)
            {
                table.Cell()
                    .Background("#B3202A")
                    .Padding(5)
                    .Text(header)
                    .FontColor("#FFFFFF")
                    .Bold()
                    .FontSize(8);
            }
        }


        private static IContainer TableCell(
            IContainer container)
        {
            return container
                .BorderBottom(1)
                .BorderColor("#DDDDDD")
                .Padding(5);
        }


        private static void ComposeFooter(
            IContainer container)
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
                            text.Span("Page ")
                                .FontSize(8);

                            text.CurrentPageNumber()
                                .FontSize(8);

                            text.Span(" of ")
                                .FontSize(8);

                            text.TotalPages()
                                .FontSize(8);
                        });
                });
        }
    }
}
