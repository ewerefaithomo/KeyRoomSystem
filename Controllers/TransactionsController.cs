
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KeyRoomSystem.Data;
using KeyRoomSystem.Models;

namespace KeyRoomSystem.Controllers
{
    public class TransactionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TransactionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool IsLoggedIn()
        {
            return !string.IsNullOrEmpty(
                HttpContext.Session.GetString("UserSession"));
        }

        // =========================================================
        // GET: /Transactions/Collect
        // Display Key Collection Page
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Collect()
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            // Load only available keys.
            var availableKeys = await _context.KeyItems
                .AsNoTracking()
                .Where(k => k.IsAvailable)
                .OrderBy(k => k.Id)
                .ToListAsync();

            ViewBag.Keys = availableKeys;

            // Staff are retrieved on demand through SearchStaff.
            return View();
        }

        // =========================================================
        // GET: /Transactions/SearchStaff?term=...
        // Fast staff autocomplete endpoint
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> SearchStaff(string? term)
        {
            if (!IsLoggedIn())
            {
                return Unauthorized();
            }

            term = term?.Trim();

            // Avoid broad queries for empty or one-character searches.
            if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
            {
                return Json(Array.Empty<object>());
            }

            // Search by staff number, name, or department.
            // Return only the fields required by the collection page.
            var results = await _context.Staffs
                .AsNoTracking()
                .Where(s =>
                    s.StaffNumber.Contains(term) ||
                    (s.FullName != null &&
                     s.FullName.Contains(term)) ||
                    (s.Department != null &&
                     s.Department.Contains(term)))
                .OrderBy(s =>
                    s.StaffNumber == term ? 0 :
                    (s.FullName != null &&
                     s.FullName.StartsWith(term)) ? 1 :
                    (s.StaffNumber.StartsWith(term)) ? 2 : 3)
                .ThenBy(s => s.FullName)
                .ThenBy(s => s.StaffNumber)
                .Select(s => new
                {
                    staffNumber = s.StaffNumber,
                    fullName = s.FullName,
                    department = s.Department
                })
                .Take(10)
                .ToListAsync();

            return Json(results);
        }

        // =========================================================
        // POST: /Transactions/Collect
        // Record Key Collection
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Collect(int keyId, string? staffNumber)
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            if (keyId <= 0)
            {
                TempData["ErrorMessage"] = "Please select a key.";
                return RedirectToAction(nameof(Collect));
            }

            if (string.IsNullOrWhiteSpace(staffNumber))
            {
                TempData["ErrorMessage"] =
                    "Please search for and select a registered staff member.";

                return RedirectToAction(nameof(Collect));
            }

            staffNumber = staffNumber.Trim();

            var key = _context.KeyItems
                .FirstOrDefault(k => k.Id == keyId);

            if (key == null)
            {
                TempData["ErrorMessage"] =
                    "The selected key could not be found.";

                return RedirectToAction(nameof(Collect));
            }

            if (!key.IsAvailable)
            {
                TempData["ErrorMessage"] =
                    $"Key Number {key.KeyNumber} is already collected.";

                return RedirectToAction(nameof(Collect));
            }

            var staff = _context.Staffs
                .FirstOrDefault(s => s.StaffNumber == staffNumber);

            if (staff == null)
            {
                TempData["ErrorMessage"] =
                    "The selected staff member could not be found.";

                return RedirectToAction(nameof(Collect));
            }

            using var databaseTransaction =
                _context.Database.BeginTransaction();

            try
            {
                var keyTransaction = new KeyTransaction
                {
                    KeyId = key.Id,
                    StaffNumber = staff.StaffNumber,
                    DateCollected = DateTime.Now,
                    DateReturned = null,
                    Status = "Collected"
                };

                _context.KeyTransactions.Add(keyTransaction);
                key.IsAvailable = false;

                _context.SaveChanges();
                databaseTransaction.Commit();

                TempData["SuccessMessage"] =
                    $"Key Number {key.KeyNumber} has been successfully " +
                    $"collected by {staff.FullName ?? staff.StaffNumber} " +
                    $"(Staff Number: {staff.StaffNumber}).";

                return RedirectToAction(nameof(Collect));
            }
            catch (Exception)
            {
                databaseTransaction.Rollback();

                TempData["ErrorMessage"] =
                    "The key collection transaction could not be completed. " +
                    "Please verify the key status and try again.";

                return RedirectToAction(nameof(Collect));
            }
        }

        // =========================================================
        // GET: /Transactions/ReturnKey
        // Display Currently Collected Keys
        // =========================================================
        [HttpGet]
        public IActionResult ReturnKey()
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            var activeCollections = _context.KeyTransactions
                .AsNoTracking()
                .Include(t => t.KeyItem)
                .Include(t => t.Staff)
                .Where(t => t.Status == "Collected")
                .OrderByDescending(t => t.DateCollected)
                .ToList();

            return View(activeCollections);
        }

        // =========================================================
        // POST: /Transactions/ProcessReturn
        // Process Key Return
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ProcessReturn(int id)
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            var transaction = _context.KeyTransactions
                .Include(t => t.KeyItem)
                .Include(t => t.Staff)
                .FirstOrDefault(t => t.Id == id);

            if (transaction == null)
            {
                TempData["ErrorMessage"] =
                    "The key collection transaction could not be found.";

                return RedirectToAction(nameof(ReturnKey));
            }

            if (transaction.Status != "Collected")
            {
                TempData["ErrorMessage"] =
                    "This transaction has already been returned.";

                return RedirectToAction(nameof(ReturnKey));
            }

            using var databaseTransaction =
                _context.Database.BeginTransaction();

            try
            {
                transaction.Status = "Returned";
                transaction.DateReturned = DateTime.Now;

                if (transaction.KeyItem != null)
                {
                    transaction.KeyItem.IsAvailable = true;
                }

                _context.SaveChanges();
                databaseTransaction.Commit();

                TempData["SuccessMessage"] =
                    $"Key Number {transaction.KeyItem?.KeyNumber} " +
                    "has been successfully returned.";

                return RedirectToAction(nameof(ReturnKey));
            }
            catch (Exception)
            {
                databaseTransaction.Rollback();

                TempData["ErrorMessage"] =
                    "The key return transaction could not be completed. " +
                    "Please try again.";

                return RedirectToAction(nameof(ReturnKey));
            }
        }
    }
}
