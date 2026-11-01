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

        // Display the Edit Event form
        [HttpGet]
        public async Task<IActionResult> EditEvent(int id)
        {
            string? role = HttpContext.Session.GetString("UserRole");

            if (role != "Organiser")
            {
                return RedirectToAction("Login", "Account");
            }

            string? token = HttpContext.Session.GetString("JWToken");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Account");
            }

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Get the existing event from the API
            var response = await client.GetAsync($"api/events/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Could not find that event.";
                return RedirectToAction("MyEvents");
            }

            var eventItem = await response.Content.ReadFromJsonAsync<EventViewModel>();

            if (eventItem == null)
            {
                TempData["ErrorMessage"] = "Could not load the event.";
                return RedirectToAction("MyEvents");
            }

            // Copy the existing event details into the form model
            var model = new CreateEventViewModel
            {
                EventName = eventItem.EventName,
                Description = eventItem.Description,
                EventDate = eventItem.EventDate,
                Location = eventItem.Location,
                Distance = eventItem.Distance,
                EventType = eventItem.EventType
            };
            ViewBag.EventId = id;
            return View(model);
        }

        // Send updated event details to the API
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEvent(int id, CreateEventViewModel model)
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

            string? token = HttpContext.Session.GetString("JWToken");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Account");
            }

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Update the event through the API
            var response = await client.PutAsJsonAsync($"api/events/{id}", model);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Event updated successfully!";
                return RedirectToAction("MyEvents");
            }

            string errorMessage = await response.Content.ReadAsStringAsync();

            ModelState.AddModelError(string.Empty,"Could not update the event. " + errorMessage);

            return View(model);
        }

        // Delete an event
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            string? role = HttpContext.Session.GetString("UserRole");

            if (role != "Organiser")
            {
                return RedirectToAction("Login", "Account");
            }

            string? token = HttpContext.Session.GetString("JWToken");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Account");
            }

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Send the delete request to the API
            var response = await client.DeleteAsync($"api/events/{id}");

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Event deleted successfully!";
            }
            else
            {
                string errorMessage = await response.Content.ReadAsStringAsync();

                TempData["ErrorMessage"] = "Could not delete the event. " + errorMessage;
            }

            return RedirectToAction("MyEvents");
        }

        // Display categories for a selected event
        [HttpGet]
        public async Task<IActionResult> ManageCategories(int id)
        {
            // Check that the user is an organiser
            string? role = HttpContext.Session.GetString("UserRole");

            if (role != "Organiser")
            {
                return RedirectToAction("Login", "Account");
            }

            // Get the JWT token
            string? token = HttpContext.Session.GetString("JWToken");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Account");
            }

            // Connect to the API
            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Get the event categories
            var response = await client.GetAsync($"api/events/{id}/categories");

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Could not load event categories.";
                return RedirectToAction("MyEvents");
            }

            // Convert the API response into a list
            var categories = await response.Content.ReadFromJsonAsync<List<CategoryViewModel>>();

            // Pass the event ID to the view
            ViewBag.EventId = id;

            return View(categories ?? new List<CategoryViewModel>());
        }

        // Display the Create Category form
        [HttpGet]
        public IActionResult CreateCategory(int id)
        {
            string? role = HttpContext.Session.GetString("UserRole");

            if (role != "Organiser")
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.EventId = id;

            return View(new CategoryViewModel { EventId = id });
        }

        // Send the new category to the API
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(int id, CategoryViewModel model)
        {
            string? role = HttpContext.Session.GetString("UserRole");

            if (role != "Organiser")
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.EventId = id;
                return View(model);
            }

            string? token = HttpContext.Session.GetString("JWToken");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Account");
            }

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Only send the fields expected by the API.
            var categoryData = new
            {
                model.CategoryName,
                model.Distance,
                model.AgeRange
            };

            var response = await client.PostAsJsonAsync($"api/events/{id}/categories", categoryData);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Category created successfully!";
                return RedirectToAction("ManageCategories", new { id });
            }

            string errorMessage = await response.Content.ReadAsStringAsync();

            ModelState.AddModelError(string.Empty,"Could not create the category. " + errorMessage);

            ViewBag.EventId = id;
            return View(model);
        }

        // Display the Edit Category form
        [HttpGet]
        public async Task<IActionResult> EditCategory(int id, int eventId)
        {
            string? role = HttpContext.Session.GetString("UserRole");

            if (role != "Organiser")
            {
                return RedirectToAction("Login", "Account");
            }

            string? token = HttpContext.Session.GetString("JWToken");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Account");
            }

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Get the categories belonging to this event.
            var response = await client.GetAsync($"api/events/{eventId}/categories");

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Could not load the categories.";
                return RedirectToAction("MyEvents");
            }

            var categories = await response.Content.ReadFromJsonAsync<List<CategoryViewModel>>();

            var category = categories?.FirstOrDefault(item => item.CategoryId == id);

            if (category == null)
            {
                TempData["ErrorMessage"] = "Category not found.";
                return RedirectToAction("ManageCategories", new { id = eventId });
            }

            ViewBag.EventId = eventId;

            return View(category);
        }

        // Send the updated category to the API
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(
            int id, int eventId, CategoryViewModel model)
        {
            string? role = HttpContext.Session.GetString("UserRole");

            if (role != "Organiser")
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.EventId = eventId;
                return View(model);
            }

            string? token = HttpContext.Session.GetString("JWToken");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Account");
            }

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var categoryData = new
            {
                model.CategoryName,
                model.Distance,
                model.AgeRange
            };

            var response = await client.PutAsJsonAsync($"api/categories/{id}", categoryData);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Category updated successfully!";
                return RedirectToAction("ManageCategories", new { id = eventId });
            }

            string errorMessage = await response.Content.ReadAsStringAsync();

            ModelState.AddModelError(string.Empty,"Could not update the category. " + errorMessage);

            ViewBag.EventId = eventId;
            return View(model);
        }

        // Delete a category
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id, int eventId)
        {
            // Check that the user is an organiser
            string? role = HttpContext.Session.GetString("UserRole");

            if (role != "Organiser")
            {
                return RedirectToAction("Login", "Account");
            }

            // Get the JWT token
            string? token = HttpContext.Session.GetString("JWToken");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Account");
            }

            // Connect to the API
            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            // Attach the JWT token
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            // Send the delete request to the API
            var response = await client.DeleteAsync($"api/categories/{id}");

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Category deleted successfully!";
            }
            else
            {
                string errorMessage = await response.Content.ReadAsStringAsync();

                TempData["ErrorMessage"] = "Could not delete the category. " + errorMessage;
            }

            // Return to the categories page for the same event
            return RedirectToAction("ManageCategories", new { id = eventId });
        }

        // Display enrolments for events belonging to the logged-in organiser.
        [HttpGet]
        public async Task<IActionResult> ManageEnrolments()
        {
            // Check that the user is an organiser.
            string? role = HttpContext.Session.GetString("UserRole");

            if (role != "Organiser")
            {
                return RedirectToAction("Login", "Account");
            }

            // Get the JWT token.
            string? token = HttpContext.Session.GetString("JWToken");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Account");
            }

            // Connect to the API.
            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            // Attach the JWT token.
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                // Retrieve enrolments for this organiser's events.
                var response = await client.GetAsync("api/organisers/enrolments");

                if (!response.IsSuccessStatusCode)
                {
                    ViewBag.ErrorMessage = "Could not load event enrolments.";

                    return View(new List<OrganiserEnrolmentViewModel>());
                }

                // Convert the API response into a list.
                var enrolments = await response.Content.ReadFromJsonAsync<List<OrganiserEnrolmentViewModel>>() ?? new List<OrganiserEnrolmentViewModel>();

                return View(enrolments);
            }
            catch (HttpRequestException)
            {
                ViewBag.ErrorMessage = "Unable to connect to the API. Please try again later.";

                return View(new List<OrganiserEnrolmentViewModel>());
            }
        }
    }
}
