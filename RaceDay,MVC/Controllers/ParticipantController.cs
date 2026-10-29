
using Microsoft.AspNetCore.Mvc;

namespace RaceDay.MVC.Controllers
{
    public class ParticipantController : Controller
    {
        // Display the participant dashboard.
        public IActionResult Index()
        {
            // Only logged-in participants can access this page.
            string? role = HttpContext.Session.GetString("UserRole");

            if (role != "Participant")
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }
    }
}
