
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

    }
}
