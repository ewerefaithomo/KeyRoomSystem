using Microsoft.AspNetCore.Mvc;
using KeyRoomSystem.Data;

namespace KeyRoomSystem.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            if (string.IsNullOrEmpty(
                HttpContext.Session.GetString("UserSession")))
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.TotalKeys = _context.KeyItems.Count();

            ViewBag.AvailableKeys = _context.KeyItems
                .Count(k => k.IsAvailable);

            ViewBag.CollectedKeys = _context.KeyItems
                .Count(k => !k.IsAvailable);

            ViewBag.TotalStaff = _context.Staffs.Count();

            return View();
        }
    }
}