using ClosedXML.Excel;
using iTender.Compliance.Application.DTOs;
using iTender.Compliance.Application.Interfaces;
using iTender.Compliance.Application.Interfaces.Services;

namespace iTender.Compliance.Infrastructure.Services
{
    public class ReportExcelService : IReportExcelService
    {
        private readonly IReportService _reportService;

        public ReportExcelService(
            IReportService reportService)
        {
            _reportService = reportService;
        }

        public async Task<byte[]> GenerateAsync(
            DateTime fromDate,
            DateTime toDate)
        {
            var summary = await _reportService.GetReportAsync(
                fromDate,
                toDate);

            var nonCompliantTenders =
                await _reportService.GetNonCompliantTendersAsync(
                    fromDate,
                    toDate);

            using var workbook = new XLWorkbook();

            CreateSummarySheet(
                workbook,
                summary,
                fromDate,
                toDate);

            CreateNonCompliantTendersSheet(
                workbook,
                nonCompliantTenders);

            using var stream = new MemoryStream();

            workbook.SaveAs(stream);

            return stream.ToArray();
        }


        private static void CreateSummarySheet(
            XLWorkbook workbook,
            ReportSummaryModel summary,
            DateTime fromDate,
            DateTime toDate)
        {
            var worksheet = workbook.Worksheets.Add("Report Summary");

            worksheet.Column(1).Width = 30;
            worksheet.Column(2).Width = 20;
            worksheet.Column(3).Width = 15;
            worksheet.Column(4).Width = 15;


            // =====================================================
            // TITLE
            // =====================================================

            worksheet.Cell(1, 1).Value =
                 "Compliance Report";

            worksheet.Range(1, 1, 1, 4)
                .Merge();

            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.FontSize = 18;

            worksheet.Cell(1, 1)
                .Style.Alignment
                .Horizontal = XLAlignmentHorizontalValues.Center;


            // =====================================================
            // REPORT PERIOD
            // =====================================================

            worksheet.Cell(3, 1).Value = "Report Period";
            worksheet.Cell(3, 2).Value =
                $"{fromDate:yyyy-MM-dd} to {toDate:yyyy-MM-dd}";

            worksheet.Cell(4, 1).Value = "Generated";
            worksheet.Cell(4, 2).Value = DateTime.UtcNow;

            worksheet.Cell(4, 2)
                .Style.DateFormat
                .Format = "yyyy-MM-dd HH:mm";


            // =====================================================
            // SUMMARY
            // =====================================================

            worksheet.Cell(6, 1).Value = "Summary";

            worksheet.Range(6, 1, 6, 2)
                .Merge();

            StyleSectionHeader(
                worksheet.Range(6, 1, 6, 2));


            AddSummaryRow(
                worksheet,
                7,
                "Total Tenders",
                summary.TotalTenders);

            AddSummaryRow(
                worksheet,
                8,
                "Compliance Cases",
                summary.TotalCases);

            AddSummaryRow(
                worksheet,
                9,
                "Non-Compliant",
                summary.NonCompliant);

            AddSummaryRow(
                worksheet,
                10,
                "Compliance Rate",
                $"{summary.ComplianceRate}%");

            AddSummaryRow(
                worksheet,
                11,
                "Cases Opened",
                summary.CasesOpened);

            AddSummaryRow(
                worksheet,
                12,
                "Cases Closed",
                summary.CasesClosed);

            AddSummaryRow(
                worksheet,
                13,
                "Cases In Progress",
                summary.CasesInProgress);

            AddSummaryRow(
                worksheet,
                14,
                "Cases Escalated",
                summary.CasesEscalated);


            // =====================================================
            // CASE STATUS
            // =====================================================

            var statusStartRow = 17;

            worksheet.Cell(statusStartRow, 1)
                .Value = "Case Status Breakdown";

            worksheet.Range(
                    statusStartRow,
                    1,
                    statusStartRow,
                    3)
                .Merge();

            StyleSectionHeader(
                worksheet.Range(
                    statusStartRow,
                    1,
                    statusStartRow,
                    3));


            worksheet.Cell(statusStartRow + 1, 1)
                .Value = "Status";

            worksheet.Cell(statusStartRow + 1, 2)
                .Value = "Count";

            worksheet.Cell(statusStartRow + 1, 3)
                .Value = "Percentage";

            StyleTableHeader(
                worksheet.Range(
                    statusStartRow + 1,
                    1,
                    statusStartRow + 1,
                    3));


            var row = statusStartRow + 2;

            foreach (var status in summary.StatusBreakdown)
            {
                worksheet.Cell(row, 1).Value =
                    status.Status;

                worksheet.Cell(row, 2).Value =
                    status.Count;

                worksheet.Cell(row, 3).Value =
                    $"{status.Percentage}%";

                row++;
            }


            // =====================================================
            // COMPLIANCE OUTCOMES
            // =====================================================

            var outcomeStartRow = row + 2;

            worksheet.Cell(outcomeStartRow, 1)
                .Value = "Compliance Outcome Breakdown";

            worksheet.Range(
                    outcomeStartRow,
                    1,
                    outcomeStartRow,
                    3)
                .Merge();

            StyleSectionHeader(
                worksheet.Range(
                    outcomeStartRow,
                    1,
                    outcomeStartRow,
                    3));


            worksheet.Cell(outcomeStartRow + 1, 1)
                .Value = "Outcome";

            worksheet.Cell(outcomeStartRow + 1, 2)
                .Value = "Count";

            worksheet.Cell(outcomeStartRow + 1, 3)
                .Value = "Percentage";

            StyleTableHeader(
                worksheet.Range(
                    outcomeStartRow + 1,
                    1,
                    outcomeStartRow + 1,
                    3));


            row = outcomeStartRow + 2;

            foreach (var outcome in summary.OutcomeBreakdown)
            {
                worksheet.Cell(row, 1).Value =
                    outcome.Outcome;

                worksheet.Cell(row, 2).Value =
                    outcome.Count;

                worksheet.Cell(row, 3).Value =
                    $"{outcome.Percentage}%";

                row++;
            }


            worksheet.Columns()
                .AdjustToContents();
        }


        private static void CreateNonCompliantTendersSheet(
            XLWorkbook workbook,
            List<ReportTenderModel> tenders)
        {
            var worksheet =
                workbook.Worksheets.Add("Non-Compliant Tenders");


            // =====================================================
            // TITLE
            // =====================================================

            worksheet.Cell(1, 1).Value =
                "Recent Non-Compliant Tenders";

            worksheet.Range(1, 1, 1, 6)
                .Merge();

            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.FontSize = 18;

            worksheet.Cell(1, 1)
                .Style.Alignment
                .Horizontal =
                    XLAlignmentHorizontalValues.Center;


            // =====================================================
            // HEADERS
            // =====================================================

            var headerRow = 3;

            worksheet.Cell(headerRow, 1)
                .Value = "Tender Number";

            worksheet.Cell(headerRow, 2)
                .Value = "Client";

            worksheet.Cell(headerRow, 3)
                .Value = "Advertised Date";

            worksheet.Cell(headerRow, 4)
                .Value = "Priority";

            worksheet.Cell(headerRow, 5)
                .Value = "Case Status";

            worksheet.Cell(headerRow, 6)
                .Value = "Outcome";


            StyleTableHeader(
                worksheet.Range(
                    headerRow,
                    1,
                    headerRow,
                    6));


            // =====================================================
            // DATA
            // =====================================================

            var row = headerRow + 1;

            foreach (var tender in tenders)
            {
                worksheet.Cell(row, 1)
                    .Value = tender.TenderNumber;

                worksheet.Cell(row, 2)
                    .Value = tender.ClientName;

                worksheet.Cell(row, 3)
                    .Value = tender.AdvertisedDate;

                worksheet.Cell(row, 3)
                    .Style.DateFormat
                    .Format = "yyyy-MM-dd";

                worksheet.Cell(row, 4)
                    .Value = tender.Priority.ToString();

                worksheet.Cell(row, 5)
                    .Value = tender.Status.ToString();

                worksheet.Cell(row, 6)
                    .Value = tender.Outcome.ToString();

                row++;
            }


            // =====================================================
            // TABLE
            // =====================================================

            if (tenders.Any())
            {
                var tableRange = worksheet.Range(
                    headerRow,
                    1,
                    row - 1,
                    6);

                var table =
                    tableRange.CreateTable();

                table.Theme =
                    XLTableTheme.TableStyleMedium2;
            }


            worksheet.Columns()
                .AdjustToContents();

            worksheet.Column(1).Width = 25;
            worksheet.Column(2).Width = 35;
            worksheet.Column(3).Width = 18;
            worksheet.Column(4).Width = 15;
            worksheet.Column(5).Width = 20;
            worksheet.Column(6).Width = 25;
        }


        private static void AddSummaryRow(
     IXLWorksheet worksheet,
     int row,
     string label,
     object value)
        {
            worksheet.Cell(row, 1)
                .Value = label;

            worksheet.Cell(row, 2)
                .Value = XLCellValue.FromObject(value);

            worksheet.Cell(row, 1)
                .Style.Font
                .Bold = true;
        }


        private static void StyleSectionHeader(
            IXLRange range)
        {
            range.Style.Font
                .SetBold();

            range.Style.Font
                .SetFontSize(12);

            range.Style.Fill
                .SetBackgroundColor(
                    XLColor.LightGray);
        }


        private static void StyleTableHeader(IXLRange range)
        {
            range.Style.Font.Bold = true;

            range.Style.Fill
                .SetBackgroundColor(XLColor.LightGray);

            range.Style.Border.BottomBorder =
                XLBorderStyleValues.Thin;
        }
    }
}