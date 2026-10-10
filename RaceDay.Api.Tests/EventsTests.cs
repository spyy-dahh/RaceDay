using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace RaceDay.Api.Tests
{
    public class EventsTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public EventsTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        // Registers a user and returns their JWT token.
        private async Task<string> RegisterAndLogin(string role)
        {
            string email = $"{Guid.NewGuid():N}@example.com";
            string password = "TestPassword123!";

            var registerRequest = new
            {
                UserName = "EventTestUser",
                EmailAddress = email,
                ContactNumber = "0712345678",
                Password = password,
                Role = role
            };

            var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

            registerResponse.EnsureSuccessStatusCode();

            var loginRequest = new
            {
                EmailAddress = email,
                Password = password
            };

            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

            loginResponse.EnsureSuccessStatusCode();

            using JsonDocument document = JsonDocument.Parse(await loginResponse.Content.ReadAsStringAsync());

            return document.RootElement.GetProperty("token").GetString()!;
        }

        // Creates an event request with valid test data.
        private static object CreateEventRequest(
            string eventType = "Run")
        {
            return new
            {
                EventName = "Test Running Event",
                Description = "An event created during testing.",
                EventDate = DateTime.UtcNow.AddDays(30),
                Location = "Pretoria",
                Distance = 10.0,
                EventType = eventType
            };
        }

        // Test 1: All events can be retrieved.
        [Fact]
        public async Task GetEvents_ReturnsOk()
        {
            var response = await _client.GetAsync("/api/events");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // Test 2: A missing event returns NotFound.
        [Fact]
        public async Task GetEvent_UnknownId_ReturnsNotFound()
        {
            var response = await _client.GetAsync("/api/events/999999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // Test 3: An organiser can create an event.
        [Fact]
        public async Task CreateEvent_Organiser_ReturnsCreated()
        {
            string token = await RegisterAndLogin("Organiser");

            using var request = new HttpRequestMessage(HttpMethod.Post, "/api/events");

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            request.Content = JsonContent.Create(CreateEventRequest());

            var response = await _client.SendAsync(request);

            Assert.Equal(HttpStatusCode.Created,response.StatusCode);
        }

        // Test 4: A participant cannot create an event.
        [Fact]
        public async Task CreateEvent_Participant_ReturnsForbidden()
        {
            string token = await RegisterAndLogin("Participant");

            using var request = new HttpRequestMessage(HttpMethod.Post, "/api/events");

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            request.Content = JsonContent.Create(CreateEventRequest());

            var response = await _client.SendAsync(request);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        // Test 5: An invalid event type is rejected.
        [Fact]
        public async Task CreateEvent_InvalidType_ReturnsBadRequest()
        {
            string token = await RegisterAndLogin("Organiser");

            using var request = new HttpRequestMessage(HttpMethod.Post, "/api/events");

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            request.Content = JsonContent.Create(CreateEventRequest("Swimming"));

            var response = await _client.SendAsync(request);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
