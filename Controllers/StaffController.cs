using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KeyRoomSystem.Data;
using KeyRoomSystem.Models;

namespace KeyRoomSystem.Controllers
{
    public class StaffController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StaffController(ApplicationDbContext context)
        {
            _context = context;
        }


        // =========================================================
        // GET: /Staff/Index
        // Display all registered staff
        // =========================================================
        [HttpGet]
        public IActionResult Index()
        {
            if (string.IsNullOrEmpty(
                HttpContext.Session.GetString("UserSession")))
            {
                return RedirectToAction("Login", "Account");
            }

            var staffs = _context.Staffs
                .OrderBy(s => s.FullName)
                .ToList();

            return View(staffs);
        }


        // =========================================================
        // GET: /Staff/Search
        // Display Staff Number Search Page
        // =========================================================
        [HttpGet]
        public IActionResult Search()
        {
            if (string.IsNullOrEmpty(
                HttpContext.Session.GetString("UserSession")))
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }


        // =========================================================
        // POST: /Staff/Search
        // Search for Staff Number
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Search(string staffNumber)
        {
            if (string.IsNullOrEmpty(
                HttpContext.Session.GetString("UserSession")))
            {
                return RedirectToAction("Login", "Account");
            }

            if (string.IsNullOrWhiteSpace(staffNumber))
            {
                TempData["ErrorMessage"] =
                    "Please enter a Staff Number.";

                return View();
            }

            staffNumber = staffNumber.Trim();

            // Search the Staffs table using StaffNumber.
            var existingStaff = _context.Staffs
                .FirstOrDefault(s =>
                    s.StaffNumber == staffNumber);

            // -----------------------------------------------------
            // STAFF ALREADY EXISTS
            // -----------------------------------------------------

            if (existingStaff != null)
            {
                TempData["ErrorMessage"] =
                    $"Staff Number {existingStaff.StaffNumber} " +
                    $"is already registered.";

                ViewBag.ExistingStaff = existingStaff;

                return View();
            }

            // -----------------------------------------------------
            // STAFF DOES NOT EXIST
            // Display Registration Page
            // -----------------------------------------------------

            var staff = new Staff
            {
                StaffNumber = staffNumber
            };

            LoadDepartments();

            return View("Register", staff);
        }


        // =========================================================
        // GET: /Staff/Register
        // Display Staff Registration Page
        // =========================================================
        [HttpGet]
        public IActionResult Register(string staffNumber)
        {
            if (string.IsNullOrEmpty(
                HttpContext.Session.GetString("UserSession")))
            {
                return RedirectToAction("Login", "Account");
            }

            if (string.IsNullOrWhiteSpace(staffNumber))
            {
                return RedirectToAction(nameof(Search));
            }

            staffNumber = staffNumber.Trim();

            // Prevent registration of an existing Staff Number.
            var existingStaff = _context.Staffs
                .FirstOrDefault(s =>
                    s.StaffNumber == staffNumber);

            if (existingStaff != null)
            {
                TempData["ErrorMessage"] =
                    $"Staff Number {existingStaff.StaffNumber} " +
                    $"is already registered.";

                return RedirectToAction(nameof(Search));
            }

            var staff = new Staff
            {
                StaffNumber = staffNumber
            };

            LoadDepartments();

            return View(staff);
        }


        // =========================================================
        // POST: /Staff/Register
        // Register New Staff
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(Staff staff)
        {
            if (string.IsNullOrEmpty(
                HttpContext.Session.GetString("UserSession")))
            {
                return RedirectToAction("Login", "Account");
            }

            // Remove leading/trailing spaces.
            staff.StaffNumber =
                staff.StaffNumber?.Trim() ?? string.Empty;

            staff.FullName =
                staff.FullName?.Trim();

            staff.Department =
                staff.Department?.Trim();

            staff.PhoneNumber =
                staff.PhoneNumber?.Trim();


            // -----------------------------------------------------
            // Validate Staff Number
            // -----------------------------------------------------

            if (string.IsNullOrWhiteSpace(staff.StaffNumber))
            {
                ModelState.AddModelError(
                    nameof(Staff.StaffNumber),
                    "Staff Number is required.");
            }


            // -----------------------------------------------------
            // Check whether Staff Number already exists
            // -----------------------------------------------------

            var existingStaff = _context.Staffs
                .FirstOrDefault(s =>
                    s.StaffNumber == staff.StaffNumber);

            if (existingStaff != null)
            {
                ModelState.AddModelError(
                    nameof(Staff.StaffNumber),
                    "This Staff Number is already registered.");
            }


            // -----------------------------------------------------
            // Validate Model
            // -----------------------------------------------------

            if (!ModelState.IsValid)
            {
                LoadDepartments();
                return View(staff);
            }


            // -----------------------------------------------------
            // Register Staff
            // -----------------------------------------------------

            try
            {
                _context.Staffs.Add(staff);

                _context.SaveChanges();

                TempData["SuccessMessage"] =
                    "Staff registration is successful";

                return RedirectToAction(nameof(RegisterSuccess));
            }
            catch (DbUpdateException)
            {
                // This also protects against duplicate StaffNumber
                // insertion if another user registered the same
                // Staff Number at almost the same time.

                ModelState.AddModelError(
                    nameof(Staff.StaffNumber),
                    "The Staff Number could not be registered. " +
                    "It may already exist in the system.");

                LoadDepartments();

                return View(staff);
            }
        }


        // =========================================================
        // GET: /Staff/RegisterSuccess
        // Display Successful Registration Message
        // =========================================================
        [HttpGet]
        public IActionResult RegisterSuccess()
        {
            if (string.IsNullOrEmpty(
                HttpContext.Session.GetString("UserSession")))
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }


        // =========================================================
        // Load Department List
        // =========================================================
        private void LoadDepartments()
        {
            var departments = _context.Staffs
                .Where(s => s.Department != null &&
                            s.Department != "")
                .Select(s => s.Department!)
                .Distinct()
                .OrderBy(d => d)
                .ToList();

            ViewBag.Departments = departments;
        }
    }
}