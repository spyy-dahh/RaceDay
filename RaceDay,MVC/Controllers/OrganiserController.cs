using System.Net.Http.Headers;
using System.Net.Http.Json;
using RaceDay.MVC.Models;
using Microsoft.AspNetCore.Mvc;

namespace RaceDay.MVC.Controllers
{
    public class OrganiserController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public OrganiserController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
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

        // Display the Create Event form
        [HttpGet]
        public IActionResult CreateEvent()
        {
            string? role = HttpContext.Session.GetString("UserRole");

            if (role != "Organiser")
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        // Send the new event to the API
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEvent(CreateEventViewModel model)
        {
            string? role = HttpContext.Session.GetString("UserRole");

            if (role != "Organiser")
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Get the JWT token saved during login
            string? token = HttpContext.Session.GetString("JWToken");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Account");
            }

            // Connect to the API
            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            // Attach the JWT token to the request
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Send the event details to the API
            var response = await client.PostAsJsonAsync("api/events", model);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Event created successfully!";

                return RedirectToAction("Index");
            }

            // Read the error returned by the API
            string errorMessage = await response.Content.ReadAsStringAsync();

            ModelState.AddModelError(string.Empty,"Could not create the event. " + errorMessage);

            return View(model);
        }

        // Display events created by the logged-in organiser
        [HttpGet]
        public async Task<IActionResult> MyEvents()
        {
            // Check that the user is an organiser
            string? role = HttpContext.Session.GetString("UserRole");

            if (role != "Organiser")
            {
                return RedirectToAction("Login", "Account");
            }

            // Get the JWT token saved during login
            string? token = HttpContext.Session.GetString("JWToken");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Account");
            }

            // Connect to the API
            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            // Attach the JWT token
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Request the organiser's events from the API
            var response = await client.GetAsync("api/organisers/events");

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Could not load your events.";
                return RedirectToAction("Index");
            }

            // Convert the API response into a list of EventViewModels
            var events = await response.Content.ReadFromJsonAsync<List<EventViewModel>>();

            return View(events ?? new List<EventViewModel>());
        }
    }
}
