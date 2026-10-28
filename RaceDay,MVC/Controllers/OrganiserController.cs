
using Microsoft.AspNetCore.Mvc;

namespace RaceDay.MVC.Controllers
{
    public class OrganiserController : Controller
    {
        // Display the organiser dashboard.
        public IActionResult Index()
        {
            // Only logged-in organisers can access this page.
            string? role = HttpContext.Session.GetString("UserRole");

            if (role != "Organiser")
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }
    }
}
