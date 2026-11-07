using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using RaceDay.MVC.Models;

namespace RaceDay.MVC.Controllers
{
    public class ParticipantController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public ParticipantController(IHttpClientFactory httpClientFactory)
        {
                _httpClientFactory = httpClientFactory;
        }
        
        // Display the participant dashboard.
        public IActionResult Index()
        {
            string? role = HttpContext.Session.GetString("UserRole");
            
            if (role != "Participant")
            {
                return RedirectToAction("Login", "Account");
            }

                return View();
        }

        // Display enrolments belonging to the logged-in participant.
        public async Task<IActionResult> MyEnrolments()
        {
            // Check that the logged-in user is a participant.
            string? role = HttpContext.Session.GetString("UserRole");

            if (role != "Participant")
            {
                return RedirectToAction("Login", "Account");
            }

            // Retrieve the JWT token.
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
                // Retrieve the participant's enrolments.
                var response = await client.GetAsync("api/participants/myEnrolments");

                if (!response.IsSuccessStatusCode)
                {
                    ViewBag.ErrorMessage = "Could not load your enrolments. Please try again.";
                    return View(new List<MyEnrolmentViewModel>());
                }

                var enrolments = await response.Content.ReadFromJsonAsync<List<MyEnrolmentViewModel>>() ?? new List<MyEnrolmentViewModel>();

                return View(enrolments);
            }
            catch (HttpRequestException)
            {
                ViewBag.ErrorMessage = "Unable to connect to the API. Please try again later.";

                return View(new List<MyEnrolmentViewModel>());
            }
        }

        // Display the participant's results
        [HttpGet]
        public async Task<IActionResult> MyResults()
        {
            // Check that the logged-in user is a participant
            if (HttpContext.Session.GetString("UserRole") != "Participant")
            {
                return RedirectToAction("Login", "Account");
            }

            // Get the JWT token
            var token = HttpContext.Session.GetString("JWToken");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Account");
            }

            // Connect to the API
            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                // Retrieve the participant's results
                var response = await client.GetAsync("api/results/myResults");

                if (!response.IsSuccessStatusCode)
                {
                    ViewBag.ErrorMessage = "Could not retrieve your results.";

                    return View(new List<MyResultViewModel>());
                }

                var results = await response.Content.ReadFromJsonAsync<List<MyResultViewModel>>();

                return View(results ?? new List<MyResultViewModel>());
            }
            catch (HttpRequestException)
            {
                ViewBag.ErrorMessage = "Unable to connect to the RaceDay API.";

                return View(new List<MyResultViewModel>());
            }
        }
    }
}

