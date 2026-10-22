using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KeyRoomSystem.Data;
using KeyRoomSystem.Models;

namespace KeyRoomSystem.Controllers
{
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // GET: /Reports
        // Main Reporting Page
        // =========================================================
        [HttpGet]
        public IActionResult Index(
            string reportType = "not-returned-today",
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            if (string.IsNullOrEmpty(
                HttpContext.Session.GetString("UserSession")))
            {
                return RedirectToAction("Login", "Account");
            }

            // -----------------------------------------------------
            // Default dates
            // -----------------------------------------------------

            DateTime today = DateTime.Today;

            DateTime selectedStartDate =
                startDate?.Date ?? today;

            DateTime selectedEndDate =
                endDate?.Date ?? today;

            // Ensure End Date is not before Start Date.
            if (selectedEndDate < selectedStartDate)
            {
                selectedEndDate = selectedStartDate;
            }

            // -----------------------------------------------------
            // View Model
            // -----------------------------------------------------

            var model = new ReportViewModel
            {
                ReportType = reportType,
                StartDate = selectedStartDate,
                EndDate = selectedEndDate
            };

            // -----------------------------------------------------
            // Generate requested report
            // -----------------------------------------------------

            switch (reportType)
            {
                case "not-returned-today":

                    model.ReportTitle =
                        "Details of Keys Not Returned for Today";

                    model.Transactions =
                        GetKeysNotReturnedForDay(today);

                    break;


                case "not-returned-period":

                    model.ReportTitle =
                        "Details of Keys Not Returned for Specified Period";

                    model.Transactions =
                        GetKeysNotReturnedForPeriod(
                            selectedStartDate,
                            selectedEndDate);

                    break;


                case "collected-period":

                    model.ReportTitle =
                        "Report of Keys Collected for Specified Period";

                    model.Transactions =
                        GetKeysCollectedForPeriod(
                            selectedStartDate,
                            selectedEndDate);

                    break;


                default:

                    model.ReportType =
                        "not-returned-today";

                    model.ReportTitle =
                        "Details of Keys Not Returned for Today";

                    model.Transactions =
                        GetKeysNotReturnedForDay(today);

                    break;
            }

            model.TotalRecords =
                model.Transactions.Count;

            return View(model);
        }


        // =========================================================
        // REPORT 1
        // Keys Not Returned for Today
        // =========================================================
        private List<KeyTransaction> GetKeysNotReturnedForDay(
            DateTime date)
        {
            DateTime start =
                date.Date;

            DateTime end =
                date.Date.AddDays(1);

            return _context.KeyTransactions
                .AsNoTracking()
                .Include(t => t.KeyItem)
                .Include(t => t.Staff)
                .Where(t =>
                    t.DateCollected >= start &&
                    t.DateCollected < end &&
                    t.DateReturned == null &&
                    t.Status == "Collected")
                .OrderBy(t => t.DateCollected)
                .ToList();
        }


        // =========================================================
        // REPORT 2
        // Keys Not Returned for Specified Period
        // =========================================================
        private List<KeyTransaction> GetKeysNotReturnedForPeriod(
            DateTime startDate,
            DateTime endDate)
        {
            DateTime start =
                startDate.Date;

            DateTime end =
                endDate.Date.AddDays(1);

            return _context.KeyTransactions
                .AsNoTracking()
                .Include(t => t.KeyItem)
                .Include(t => t.Staff)
                .Where(t =>
                    t.DateCollected >= start &&
                    t.DateCollected < end &&
                    t.DateReturned == null &&
                    t.Status == "Collected")
                .OrderBy(t => t.DateCollected)
                .ToList();
        }


        // =========================================================
        // REPORT 3
        // Keys Collected for Specified Period
        // =========================================================
        private List<KeyTransaction> GetKeysCollectedForPeriod(
            DateTime startDate,
            DateTime endDate)
        {
            DateTime start =
                startDate.Date;

            DateTime end =
                endDate.Date.AddDays(1);

            return _context.KeyTransactions
                .AsNoTracking()
                .Include(t => t.KeyItem)
                .Include(t => t.Staff)
                .Where(t =>
                    t.DateCollected >= start &&
                    t.DateCollected < end)
                .OrderBy(t => t.DateCollected)
                .ToList();
        }


        // =========================================================
        // EXPORT TO EXCEL
        // =========================================================
        [HttpGet]
        public IActionResult ExportExcel(
            string reportType = "not-returned-today",
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            if (string.IsNullOrEmpty(
                HttpContext.Session.GetString("UserSession")))
            {
                return RedirectToAction("Login", "Account");
            }

            DateTime today =
                DateTime.Today;

            DateTime selectedStartDate =
                startDate?.Date ?? today;

            DateTime selectedEndDate =
                endDate?.Date ?? today;

            if (selectedEndDate < selectedStartDate)
            {
                selectedEndDate = selectedStartDate;
            }

            List<KeyTransaction> transactions;

            string reportTitle;

            switch (reportType)
            {
                case "not-returned-period":

                    reportTitle =
                        "Details of Keys Not Returned for Specified Period";

                    transactions =
                        GetKeysNotReturnedForPeriod(
                            selectedStartDate,
                            selectedEndDate);

                    break;


                case "collected-period":

                    reportTitle =
                        "Report of Keys Collected for Specified Period";

                    transactions =
                        GetKeysCollectedForPeriod(
                            selectedStartDate,
                            selectedEndDate);

                    break;


                case "not-returned-today":
                default:

                    reportTitle =
                        "Details of Keys Not Returned for Today";

                    transactions =
                        GetKeysNotReturnedForDay(today);

                    break;
            }

            using var workbook =
                new XLWorkbook();

            var worksheet =
                workbook.Worksheets.Add("Key Room Report");


            // =====================================================
            // TITLE
            // =====================================================

            worksheet.Cell(1, 1).Value =
                "KEY ROOM MANAGEMENT SYSTEM";

            worksheet.Range(1, 1, 1, 9)
                .Merge();

            worksheet.Cell(1, 1)
                .Style.Font.Bold = true;

            worksheet.Cell(1, 1)
                .Style.Font.FontSize = 16;

            worksheet.Cell(1, 1)
                .Style.Alignment.Horizontal =
                    XLAlignmentHorizontalValues.Center;


            // =====================================================
            // REPORT TITLE
            // =====================================================

            worksheet.Cell(2, 1).Value =
                reportTitle;

            worksheet.Range(2, 1, 2, 9)
                .Merge();

            worksheet.Cell(2, 1)
                .Style.Font.Bold = true;

            worksheet.Cell(2, 1)
                .Style.Font.FontSize = 13;

            worksheet.Cell(2, 1)
                .Style.Alignment.Horizontal =
                    XLAlignmentHorizontalValues.Center;


            // =====================================================
            // REPORT PERIOD
            // =====================================================

            string periodText;

            if (reportType == "not-returned-today")
            {
                periodText =
                    $"Date: {today:dd/MM/yyyy}";
            }
            else
            {
                periodText =
                    $"Period: {selectedStartDate:dd/MM/yyyy} " +
                    $"to {selectedEndDate:dd/MM/yyyy}";
            }

            worksheet.Cell(3, 1).Value =
                periodText;

            worksheet.Range(3, 1, 3, 9)
                .Merge();

            worksheet.Cell(3, 1)
                .Style.Alignment.Horizontal =
                    XLAlignmentHorizontalValues.Center;


            // =====================================================
            // REPORT HEADERS
            // =====================================================

            int headerRow = 5;

            worksheet.Cell(headerRow, 1).Value =
                "S/N";

            worksheet.Cell(headerRow, 2).Value =
                "Key Number";

            worksheet.Cell(headerRow, 3).Value =
                "Room Name";

            worksheet.Cell(headerRow, 4).Value =
                "Staff Number";

            worksheet.Cell(headerRow, 5).Value =
                "Staff Name";

            worksheet.Cell(headerRow, 6).Value =
                "Department";

            worksheet.Cell(headerRow, 7).Value =
                "Date Collected";

            worksheet.Cell(headerRow, 8).Value =
                "Date Returned";

            worksheet.Cell(headerRow, 9).Value =
                "Status";


            // =====================================================
            // HEADER FORMATTING
            // =====================================================

            var headerRange =
                worksheet.Range(
                    headerRow,
                    1,
                    headerRow,
                    9);

            headerRange.Style.Font.Bold =
                true;

            headerRange.Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;

            headerRange.Style.Alignment.Vertical =
                XLAlignmentVerticalValues.Center;


            // =====================================================
            // DATA
            // =====================================================

            int row = headerRow + 1;

            int serialNumber = 1;

            foreach (var transaction in transactions)
            {
                worksheet.Cell(row, 1).Value =
                    serialNumber++;

                worksheet.Cell(row, 2).Value =
                    transaction.KeyItem?.KeyNumber ?? "";

                worksheet.Cell(row, 3).Value =
                    transaction.KeyItem?.RoomName ?? "";

                worksheet.Cell(row, 4).Value =
                    transaction.StaffNumber;

                worksheet.Cell(row, 5).Value =
                    transaction.Staff?.FullName
                    ?? "";

                worksheet.Cell(row, 6).Value =
                    transaction.Staff?.Department
                    ?? "";

                worksheet.Cell(row, 7).Value =
                    transaction.DateCollected;

                worksheet.Cell(row, 7)
                    .Style.DateFormat.Format =
                    "dd/MM/yyyy HH:mm";

                if (transaction.DateReturned.HasValue)
                {
                    worksheet.Cell(row, 8).Value =
                        transaction.DateReturned.Value;

                    worksheet.Cell(row, 8)
                        .Style.DateFormat.Format =
                        "dd/MM/yyyy HH:mm";
                }
                else
                {
                    worksheet.Cell(row, 8).Value =
                        "";
                }

                worksheet.Cell(row, 9).Value =
                    transaction.Status;

                row++;
            }


            // =====================================================
            // TOTAL
            // =====================================================

            int totalRow = row + 1;

            worksheet.Cell(totalRow, 1).Value =
                "TOTAL RECORDS:";

            worksheet.Cell(totalRow, 2).Value =
                transactions.Count;

            worksheet.Cell(totalRow, 1)
                .Style.Font.Bold = true;

            worksheet.Cell(totalRow, 2)
                .Style.Font.Bold = true;


            // =====================================================
            // BORDERS
            // =====================================================

            if (transactions.Count > 0)
            {
                worksheet.Range(
                    headerRow,
                    1,
                    row - 1,
                    9)
                    .Style.Border
                    .OutsideBorder =
                    XLBorderStyleValues.Thin;

                worksheet.Range(
                    headerRow,
                    1,
                    row - 1,
                    9)
                    .Style.Border
                    .InsideBorder =
                    XLBorderStyleValues.Thin;
            }


            // =====================================================
            // COLUMN WIDTH
            // =====================================================

            worksheet.Columns()
                .AdjustToContents();

            worksheet.Column(1)
                .Width = 8;

            worksheet.Column(2)
                .Width = 15;

            worksheet.Column(3)
                .Width = 30;

            worksheet.Column(4)
                .Width = 18;

            worksheet.Column(5)
                .Width = 30;

            worksheet.Column(6)
                .Width = 30;

            worksheet.Column(7)
                .Width = 22;

            worksheet.Column(8)
                .Width = 22;

            worksheet.Column(9)
                .Width = 15;


            // =====================================================
            // FREEZE HEADER
            // =====================================================

            worksheet.SheetView.FreezeRows(
                headerRow);


            // =====================================================
            // FILTER
            // =====================================================

            if (transactions.Count > 0)
            {
                worksheet.Range(
                    headerRow,
                    1,
                    row - 1,
                    9)
                    .SetAutoFilter();
            }


            // =====================================================
            // PREPARE FILE
            // =====================================================

            using var stream =
                new MemoryStream();

            workbook.SaveAs(stream);

            stream.Position = 0;


            string safeFileName =
                reportType switch
                {
                    "not-returned-period" =>
                        $"Keys_Not_Returned_{selectedStartDate:yyyyMMdd}_{selectedEndDate:yyyyMMdd}.xlsx",

                    "collected-period" =>
                        $"Keys_Collected_{selectedStartDate:yyyyMMdd}_{selectedEndDate:yyyyMMdd}.xlsx",

                    _ =>
                        $"Keys_Not_Returned_{today:yyyyMMdd}.xlsx"
                };


            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                safeFileName);
        }
    }
}