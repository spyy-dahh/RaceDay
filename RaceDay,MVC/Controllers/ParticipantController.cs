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

        // Display the participant's profile.
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            // Check that the logged-in user is a participant.
            if (HttpContext.Session.GetString("UserRole") != "Participant")
            {
                return RedirectToAction("Login", "Account");
            }

            // Retrieve the JWT token.
            string? token = HttpContext.Session.GetString("JWToken");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Account");
            }

            // Connect to the API.
            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            // Retrieve the participant's profile.
            var response = await client.GetAsync("api/users/profile");

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Could not load your profile.";
                return RedirectToAction("Index");
            }

            var profile = await response.Content.ReadFromJsonAsync<ProfileViewModel>();

            if (profile == null)
            {
                TempData["ErrorMessage"] = "Your profile could not be found.";
                return RedirectToAction("Index");
            }

            return View(profile);
        }


        // Display the Edit Profile form.
        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            if (HttpContext.Session.GetString("UserRole") != "Participant")
            {
                return RedirectToAction("Login", "Account");
            }

            string? token = HttpContext.Session.GetString("JWToken");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Account");
            }

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            // Retrieve the existing profile information.
            var response = await client.GetAsync("api/users/profile");

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Could not load your profile.";
                return RedirectToAction("Index");
            }

            var profile = await response.Content.ReadFromJsonAsync<ProfileViewModel>();

            if (profile == null)
            {
                TempData["ErrorMessage"] = "Your profile could not be found.";
                return RedirectToAction("Index");
            }

            return View(profile);
        }


        // Submit the updated profile information.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(ProfileViewModel model)
        {
            if (HttpContext.Session.GetString("UserRole") != "Participant")
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

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            // Send the updated contact number to the API.
            var updateData = new
            {
                model.ContactNumber
            };

            var response = await client.PutAsJsonAsync(
                "api/users/profile", updateData);

            if (!response.IsSuccessStatusCode)
            {
                string errorMessage = await response.Content.ReadAsStringAsync();

                ModelState.AddModelError(
                    string.Empty,
                    "Could not update your profile. " + errorMessage);

                return View(model);
            }

            TempData["SuccessMessage"] = "Profile updated successfully!";

            return RedirectToAction("Profile");
        }

        // Upload or replace the participant's profile picture.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadProfileImage(IFormFile image)
        {
            // Check that the logged-in user is a participant.
            if (HttpContext.Session.GetString("UserRole") != "Participant")
            {
                return RedirectToAction("Login", "Account");
            }

            // Retrieve the JWT token.
            string? token = HttpContext.Session.GetString("JWToken");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Account");
            }

            // Check that an image was selected.
            if (image == null || image.Length == 0)
            {
                TempData["ErrorMessage"] = "Please select an image.";
                return RedirectToAction("Profile");
            }

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            // Prepare the image to send to the API.
            using var formData = new MultipartFormDataContent();

            using var imageStream = image.OpenReadStream();

            using var imageContent = new StreamContent(imageStream);

            imageContent.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue(
                    string.IsNullOrEmpty(image.ContentType)
                        ? "application/octet-stream"
                        : image.ContentType);

            formData.Add(imageContent, "image", image.FileName);

            try
            {
                // Send the image to the API.
                var response = await client.PostAsync(
                    "api/users/profile/image", formData);

                if (!response.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] =
                        "Could not upload your profile picture. Please check the image and try again.";

                    return RedirectToAction("Profile");
                }

                TempData["SuccessMessage"] =
                    "Profile picture uploaded successfully!";

                return RedirectToAction("Profile");
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] =
                    "Unable to connect to the RaceDay API.";

                return RedirectToAction("Profile");
            }
        }

        // Retrieve the logged-in participant's profile picture for display.
        [HttpGet]
        public async Task<IActionResult> ProfileImage()
        {
            // Check that the logged-in user is a participant.
            if (HttpContext.Session.GetString("UserRole") != "Participant")
            {
                return Unauthorized();
            }

            // Retrieve the JWT token.
            string? token = HttpContext.Session.GetString("JWToken");

            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized();
            }

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            try
            {
                // Retrieve the picture through the API.
                var response = await client.GetAsync("api/users/profile/image");

                if (!response.IsSuccessStatusCode)
                {
                    return NotFound();
                }

                // Read the image and its content type.
                byte[] imageBytes =
                    await response.Content.ReadAsByteArrayAsync();

                string contentType =
                    response.Content.Headers.ContentType?.MediaType
                    ?? "application/octet-stream";

                return File(imageBytes, contentType);
            }
            catch (HttpRequestException)
            {
                return NotFound();
            }
        }
    }
}

