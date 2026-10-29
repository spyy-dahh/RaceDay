
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using System.Text.Json;
using RaceDay.MVC.Models;

namespace RaceDay.MVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AccountController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // Display the registration form.
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // Send the registration details to the API.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            // Stop if the form contains invalid information.
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            // Send the form details to the existing API endpoint.
            var response = await client.PostAsJsonAsync("api/auth/register",model);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Registration successful! You can now log in.";

                return RedirectToAction("Login");
            }

            // Try to read the error message returned by the API.
            string errorMessage = "Registration failed. Please try again.";

            try
            {
                var errorContent = await response.Content.ReadAsStringAsync();

                using var document = JsonDocument.Parse(errorContent);

                if (document.RootElement.TryGetProperty("message", out var message))
                {
                    errorMessage = message.GetString() ?? errorMessage;
                }
            }
            catch (JsonException)
            {
                // Keep the default message if the response isn't JSON.
            }

            ModelState.AddModelError(string.Empty, errorMessage);

            return View(model);
        }


        // Display the login form.
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // Send login details to the API.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            var response = await client.PostAsJsonAsync("api/auth/login", model);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty,"Invalid email address or password.");

                return View(model);
            }

            // Read the token and role returned by the API.
            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

            var root = document.RootElement;

            string token = root.GetProperty("token").GetString()!;
            string role = root.GetProperty("role").GetString()!;

            // Store login information for later requests.
            HttpContext.Session.SetString("JWToken", token);
            HttpContext.Session.SetString("UserRole", role);

            TempData["SuccessMessage"] = "Login successful!";

            // Send the user to the appropriate starting page.
            if (role == "Organiser")
            {
                return RedirectToAction("Index", "Organiser");
            }

            return RedirectToAction("Index", "Participant");
        }


        // Log the user out of the MVC application.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            // Remove the stored JWT, role and other session information.
            HttpContext.Session.Clear();

            TempData["SuccessMessage"] = "You have been logged out.";

            return RedirectToAction("Login", "Account");
        }

    }
}
