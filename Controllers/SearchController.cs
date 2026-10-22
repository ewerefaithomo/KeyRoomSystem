using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KeyRoomSystem.Data;
using KeyRoomSystem.Models;

namespace KeyRoomSystem.Controllers
{
    public class SearchController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SearchController(ApplicationDbContext context)
        {
            _context = context;
        }


        // =========================================================
        // GET: /Search
        // Search Page
        // =========================================================
        [HttpGet]
        public IActionResult Index(
            string? query = null,
            string searchType = "all")
        {
            // -----------------------------------------------------
            // Check Login
            // -----------------------------------------------------

            if (string.IsNullOrEmpty(
                HttpContext.Session.GetString("UserSession")))
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }


            // -----------------------------------------------------
            // Create Search View Model
            // -----------------------------------------------------

            var model = new SearchViewModel
            {
                Query = query?.Trim() ?? string.Empty,
                SearchType = searchType,
                Results = new List<KeyTransaction>(),
                StaffResults = new List<Staff>(),
                KeyResults = new List<KeyItem>()
            };


            // -----------------------------------------------------
            // If no search term was entered, display empty page.
            // -----------------------------------------------------

            if (string.IsNullOrWhiteSpace(model.Query))
            {
                return View(model);
            }


            // =====================================================
            // SEARCH STAFF
            // =====================================================

            if (searchType == "all" ||
                searchType == "staff")
            {
                model.StaffResults =
                    _context.Staffs
                        .AsNoTracking()
                        .Where(s =>
                            s.StaffNumber.Contains(model.Query) ||
                            (s.FullName != null &&
                             s.FullName.Contains(model.Query)) ||
                            (s.Department != null &&
                             s.Department.Contains(model.Query)) ||
                            (s.PhoneNumber != null &&
                             s.PhoneNumber.Contains(model.Query)))
                        .OrderBy(s => s.FullName)
                        .ThenBy(s => s.StaffNumber)
                        .ToList();
            }


            // =====================================================
            // SEARCH KEYS
            // =====================================================

            if (searchType == "all" ||
                searchType == "key")
            {
                model.KeyResults =
                    _context.KeyItems
                        .AsNoTracking()
                        .Where(k =>
                            k.KeyNumber.Contains(model.Query) ||
                            k.RoomName.Contains(model.Query) ||
                            (k.Description != null &&
                             k.Description.Contains(model.Query)))
                        .OrderBy(k => k.Id)
                        .ToList();
            }


            // =====================================================
            // SEARCH TRANSACTIONS
            // =====================================================

            if (searchType == "all" ||
                searchType == "transaction")
            {
                model.Results =
                    _context.KeyTransactions
                        .AsNoTracking()
                        .Include(t => t.KeyItem)
                        .Include(t => t.Staff)
                        .Where(t =>
                            t.StaffNumber.Contains(model.Query) ||

                            (t.Staff != null &&
                             t.Staff.FullName != null &&
                             t.Staff.FullName.Contains(model.Query)) ||

                            (t.Staff != null &&
                             t.Staff.Department != null &&
                             t.Staff.Department.Contains(model.Query)) ||

                            (t.KeyItem != null &&
                             t.KeyItem.KeyNumber.Contains(model.Query)) ||

                            (t.KeyItem != null &&
                             t.KeyItem.RoomName.Contains(model.Query)) ||

                            t.Status.Contains(model.Query))
                        .OrderByDescending(
                            t => t.DateCollected)
                        .ToList();
            }


            // -----------------------------------------------------
            // Search by transaction status specifically.
            // -----------------------------------------------------

            if (searchType == "status")
            {
                model.Results =
                    _context.KeyTransactions
                        .AsNoTracking()
                        .Include(t => t.KeyItem)
                        .Include(t => t.Staff)
                        .Where(t =>
                            t.Status.Contains(model.Query))
                        .OrderByDescending(
                            t => t.DateCollected)
                        .ToList();
            }


            // -----------------------------------------------------
            // Determine whether anything was found.
            // -----------------------------------------------------

            model.TotalResults =
                model.StaffResults.Count +
                model.KeyResults.Count +
                model.Results.Count;


            return View(model);
        }
    }
}