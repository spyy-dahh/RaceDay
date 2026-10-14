using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RaceDay.API.Data;
using RaceDay.Api.Models;
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
        private readonly CustomWebApplicationFactory _factory;

        public EventsTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
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


        [Fact]
        public async Task CreateEvent_WithoutToken_ReturnsUnauthorized()
        {
            // Arrange: Create a request to add an event.
            using var client = _factory.CreateClient();

            var request = CreateEventRequest();

            // Act: Try to create an event without logging in.
            var response = await client.PostAsJsonAsync("/api/events", request);

            // Assert: The API should reject the request.
            Assert.Equal(System.Net.HttpStatusCode.Unauthorized,response.StatusCode);
        }


        [Fact]
        public async Task UpdateEvent_AsOwner_ReturnsOk()
        {
            // Arrange: Register and log in as an organiser.
            var token = await RegisterAndLogin("Organiser");

            using var client = _factory.CreateClient();

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Create an event first.
            var createResponse = await client.PostAsJsonAsync("/api/events", CreateEventRequest());

            Assert.Equal(System.Net.HttpStatusCode.Created,createResponse.StatusCode);

            var createdEvent = await createResponse.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();

            int eventId = createdEvent.GetProperty("eventID").GetInt32();

            // Prepare the updated event details.
            var updatedEvent = new
            {
                EventName = "Updated RaceDay Event",
                Description = "Updated event description.",
                EventDate = DateTime.UtcNow.AddDays(40),
                Location = "Johannesburg",
                Distance = 15.0,
                EventType = "Run"
            };

            // Act: Update the event we just created.
            var response = await client.PutAsJsonAsync($"/api/events/{eventId}", updatedEvent);

            // Assert: The update should succeed.
            Assert.Equal(System.Net.HttpStatusCode.OK,response.StatusCode);
        }


        [Fact]
        public async Task UpdateEvent_NonexistentEvent_ReturnsNotFound()
        {
            // Arrange: Log in as an organiser.
            var token = await RegisterAndLogin("Organiser");

            using var client = _factory.CreateClient();

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var updatedEvent = CreateEventRequest();

            // Act: Try to update an event that does not exist.
            var response = await client.PutAsJsonAsync("/api/events/999999", updatedEvent);

            // Assert: The API should return 404.
            Assert.Equal(System.Net.HttpStatusCode.NotFound,response.StatusCode);
        }


        [Fact]
        public async Task UpdateEvent_NotOwner_ReturnsForbidden()
        {
            // Arrange: The first organiser creates an event.
            var firstOrganiserToken = await RegisterAndLogin("Organiser");

            using var firstClient = _factory.CreateClient();

            firstClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", firstOrganiserToken);

            var createResponse = await firstClient.PostAsJsonAsync("/api/events", CreateEventRequest());

            Assert.Equal(System.Net.HttpStatusCode.Created,createResponse.StatusCode);

            var createdEvent = await createResponse.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();

            int eventId = createdEvent.GetProperty("eventID").GetInt32();

            // The second organiser attempts to update that event.
            var secondOrganiserToken = await RegisterAndLogin("Organiser");

            using var secondClient = _factory.CreateClient();

            secondClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", secondOrganiserToken);

            var response = await secondClient.PutAsJsonAsync($"/api/events/{eventId}", CreateEventRequest());

            // Assert: The second organiser should be denied access.
            Assert.Equal(System.Net.HttpStatusCode.Forbidden,response.StatusCode);
        }


        // Test 10: An organiser can delete their own event.
        [Fact]
        public async Task DeleteEvent_AsOwner_ReturnsOk()
        {
            // Arrange: Log in as an organiser.
            string token = await RegisterAndLogin("Organiser");

            using var createRequest = new HttpRequestMessage(HttpMethod.Post, "/api/events");

            createRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            createRequest.Content = JsonContent.Create(CreateEventRequest());

            var createResponse = await _client.SendAsync(createRequest);

            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

            using JsonDocument document = JsonDocument.Parse(await createResponse.Content.ReadAsStringAsync());

            int eventId = document.RootElement.GetProperty("eventID").GetInt32();

            // Act: Delete the event we just created.
            using var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/events/{eventId}");

            deleteRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var deleteResponse = await _client.SendAsync(deleteRequest);

            // Assert: The event should be deleted successfully.
            Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
        }

        // Test 11: Deleting an event that does not exist returns NotFound.
        [Fact]
        public async Task DeleteEvent_UnknownId_ReturnsNotFound()
        {
            // Arrange: Log in as an organiser.
            string token = await RegisterAndLogin("Organiser");

            using var request = new HttpRequestMessage(HttpMethod.Delete, "/api/events/999999");

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act: Try to delete a nonexistent event.
            var response = await _client.SendAsync(request);

            // Assert: The API should return NotFound.
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // Test 12: An organiser cannot delete another organiser's event.
        [Fact]
        public async Task DeleteEvent_NotOwner_ReturnsForbidden()
        {
            // Arrange: The first organiser creates an event.
            string firstToken = await RegisterAndLogin("Organiser");

            using var createRequest = new HttpRequestMessage(HttpMethod.Post, "/api/events");

            createRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", firstToken);

            createRequest.Content = JsonContent.Create(CreateEventRequest());

            var createResponse = await _client.SendAsync(createRequest);

            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

            using JsonDocument document = JsonDocument.Parse(await createResponse.Content.ReadAsStringAsync());

            int eventId = document.RootElement.GetProperty("eventID").GetInt32();

            // Arrange: A second organiser attempts to delete the event.
            string secondToken = await RegisterAndLogin("Organiser");

            using var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/events/{eventId}");

            deleteRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", secondToken);

            // Act: Attempt to delete another organiser's event.
            var response = await _client.SendAsync(deleteRequest);

            // Assert: The second organiser should be denied.
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }


        // Test 13: An event with categories cannot be deleted.
        [Fact]
        public async Task DeleteEvent_WithCategories_ReturnsConflict()
        {
            // Arrange: Log in as an organiser.
            string token = await RegisterAndLogin("Organiser");

            using var createRequest = new HttpRequestMessage(HttpMethod.Post, "/api/events");

            createRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            createRequest.Content = JsonContent.Create(CreateEventRequest());

            var createResponse = await _client.SendAsync(createRequest);

            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

            using JsonDocument document = JsonDocument.Parse(await createResponse.Content.ReadAsStringAsync());

            int eventId = document.RootElement.GetProperty("eventID").GetInt32();

            // Add a category linked to the event in the test database.
            using (var scope = _factory.Services.CreateScope())
            {
                var database = scope.ServiceProvider.GetRequiredService<RaceDayDbContext>();

                database.EventCategories.Add(new EventCategory
                {
                    EventId = eventId,
                    CategoryName = "10K",
                    Distance = "10",
                    AgeRange = "All ages"
                });

                await database.SaveChangesAsync();
            }

            // Act: Try to delete the event that has a category.
            using var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/events/{eventId}");

            deleteRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.SendAsync(deleteRequest);

            // Assert: The API should prevent the deletion.
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

    }
}
