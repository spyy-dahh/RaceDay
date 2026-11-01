using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using RaceDay.MVC.Models;

namespace RaceDay.MVC.Controllers
{
    public class EventsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public EventsController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }


        public async Task<IActionResult> Index(string? searchTerm, string? eventType)
        {
            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            try
            {
                // Retrieve the events from the API.
                var response = await client.GetAsync("api/events");

                if (!response.IsSuccessStatusCode)
                {
                    ViewBag.ErrorMessage = "Could not load events from the API.";
                    return View(new List<EventViewModel>());
                }

                var events = await response.Content.ReadFromJsonAsync<List<EventViewModel>>() ?? new List<EventViewModel>();

                // Search by event name or location.
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    events = events.Where(eventItem => eventItem.EventName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) || eventItem.Location.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                // Filter by event type if one was selected.
                if (!string.IsNullOrWhiteSpace(eventType))
                {
                    events = events.Where(eventItem => eventItem.EventType.Equals(eventType, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                // Keep the selected filters when the page reloads.
                ViewBag.SearchTerm = searchTerm;
                ViewBag.EventType = eventType;

                return View(events);
            }
            catch (HttpRequestException)
            {
                ViewBag.ErrorMessage = "Unable to connect to the API. Please try again later.";

                return View(new List<EventViewModel>());
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            try
            {
                // Retrieve the selected event from the API.
                var eventResponse = await client.GetAsync($"api/events/{id}");

                if (!eventResponse.IsSuccessStatusCode)
                {
                    return NotFound();
                }

                var eventDetails = await eventResponse.Content.ReadFromJsonAsync<EventViewModel>();

                if (eventDetails == null)
                {
                    return NotFound();
                }

                // Retrieve the categories belonging to this event.
                var categoryResponse = await client.GetAsync($"api/events/{id}/categories");

                if (!categoryResponse.IsSuccessStatusCode)
                {
                    ViewBag.ErrorMessage = "Could not load the event categories.";
                    return View(new EventDetailsViewModel
                    {
                        Event = eventDetails
                    });
                }

                var categories = await categoryResponse.Content.ReadFromJsonAsync<List<CategoryViewModel>>() ?? new List<CategoryViewModel>();

                // Combine the event and its categories.
                var model = new EventDetailsViewModel
                {
                    Event = eventDetails,
                    Categories = categories
                };

                return View(model);
            }
            catch (HttpRequestException)
            {
                return NotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enrol(int eventId, int categoryId)
        {
            // Check that the participant is logged in.
            string? role = HttpContext.Session.GetString("UserRole");

            if (role != "Participant")
            {
                return RedirectToAction("Login", "Account");
            }

            // Get the JWT token from the session.
            string? token = HttpContext.Session.GetString("JWToken");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Account");
            }

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            // Attach the JWT token to the API request.
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                // Create the request body expected by the API.
                var enrolmentData = new
                {
                    CategoryId = categoryId
                };

                var response = await client.PostAsJsonAsync($"api/events/{eventId}/enrolments",enrolmentData);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "You have successfully enrolled in this event!";
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();

                    try
                    {
                        using var errorDocument = JsonDocument.Parse(errorContent);

                        if (errorDocument.RootElement.TryGetProperty("message", out var message))
                        {
                            TempData["ErrorMessage"] = message.GetString();
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "Could not complete your enrolment.";
                        }
                    }
                    catch (JsonException)
                    {
                        TempData["ErrorMessage"] = "Could not complete your enrolment.";
                    }
                }

                return RedirectToAction("Details", new { id = eventId });
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] = "Unable to connect to the API. Please try again later.";

                return RedirectToAction("Details", new { id = eventId });
            }
        }

    }
}
