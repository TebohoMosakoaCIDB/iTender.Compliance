using ClosedXML.Excel;
using iTender.Compliance.Application.DTOs;
using iTender.Compliance.Application.Interfaces.Services;
using iTender.Compliance.Application.Models.Reports;

namespace iTender.Compliance.Infrastructure.Services
{
    public class NonCompliantCasesExcelService : INonCompliantCasesExcelService
    {
        private readonly IReportingService _reportingService;

        public NonCompliantCasesExcelService(IReportingService reportingService)
        {
            _reportingService = reportingService;
        }

        public async Task<byte[]> GenerateAsync(
            NonCompliantCaseReportFilter filter,
            CancellationToken ct = default)
        {
            var rows = await _reportingService
                .GetNonCompliantCasesReportAsync(filter, ct);

            using var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("Non-Compliant Cases");

            sheet.Cell(1, 1).Value = "Non-Compliant Cases Report";
            sheet.Range(1, 1, 1, 11).Merge();
            sheet.Cell(1, 1).Style.Font.Bold = true;
            sheet.Cell(1, 1).Style.Font.FontSize = 16;
            sheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            sheet.Cell(2, 1).Value = "Generated";
            sheet.Cell(2, 2).Value = DateTime.UtcNow;
            sheet.Cell(2, 2).Style.DateFormat.Format = "yyyy-MM-dd HH:mm";

            var headerRow = 4;
            var headers = new[]
            {
                "Tender Number", "Title", "Employer", "Province", "Officer",
                "Status", "Findings", "Days Open", "SLA",
                "Last Letter", "Last Letter Sent", "Response Due"
            };

            for (int i = 0; i < headers.Length; i++)
                sheet.Cell(headerRow, i + 1).Value = headers[i];

            var headerRange = sheet.Range(headerRow, 1, headerRow, headers.Length);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            headerRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;

            var r = headerRow + 1;
            foreach (var row in rows)
            {
                sheet.Cell(r, 1).Value = row.TenderNumber;
                sheet.Cell(r, 2).Value = row.TenderTitle;
                sheet.Cell(r, 3).Value = row.EmployerName;
                sheet.Cell(r, 4).Value = row.Province ?? "";
                sheet.Cell(r, 5).Value = row.AgentName ?? "";
                sheet.Cell(r, 6).Value = row.Status.ToString();
                sheet.Cell(r, 7).Value = row.FindingsCount;
                sheet.Cell(r, 8).Value = row.DaysOpen;
                sheet.Cell(r, 9).Value = row.IsPastSla ? "Breached"
                                      : row.IsSlaWarning ? "Warning"
                                      : "On Track";

                sheet.Cell(r, 10).Value = row.LastLetterType ?? "";

                if (row.LastLetterSentOn.HasValue)
                {
                    sheet.Cell(r, 11).Value = row.LastLetterSentOn.Value;
                    sheet.Cell(r, 11).Style.DateFormat.Format = "yyyy-MM-dd";
                }

                if (row.ResponseDueOn.HasValue)
                {
                    sheet.Cell(r, 12).Value = row.ResponseDueOn.Value;
                    sheet.Cell(r, 12).Style.DateFormat.Format = "yyyy-MM-dd";
                }

                r++;
            }

            if (rows.Any())
            {
                var tableRange = sheet.Range(headerRow, 1, r - 1, headers.Length);
                var table = tableRange.CreateTable();
                table.Theme = XLTableTheme.TableStyleMedium2;
            }

            sheet.Columns().AdjustToContents();
            sheet.Column(1).Width = 25;
            sheet.Column(2).Width = 45;
            sheet.Column(3).Width = 35;

            using var ms = new MemoryStream();
            workbook.SaveAs(ms);
            return ms.ToArray();
        }
    }
}