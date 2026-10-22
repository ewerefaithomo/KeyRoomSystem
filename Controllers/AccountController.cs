using Microsoft.AspNetCore.Mvc;
using KeyRoomSystem.Data;

namespace KeyRoomSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            // If already logged in, redirect straight to dashboard
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("UserSession")))
            {
                return RedirectToAction("Index", "Dashboard");
            }
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            // Search the Users table for matching username and password
            var user = _context.Users.FirstOrDefault(u => u.Username == username && u.Password == password);

            if (user != null)
            {
                // Successful login: Store session and go to Dashboard
                HttpContext.Session.SetString("UserSession", user.Username);
                return RedirectToAction("Index", "Dashboard");
            }

            // Failed login: Display the required error message
            ModelState.AddModelError(string.Empty, "Incorrect Login Details");
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}