using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RaceDay.API.Data;
using RaceDay.Api.Models;
using Xunit;

namespace RaceDay.Api.Tests
{
    public class EventEnrolmentsTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory _factory;

        public EventEnrolmentsTests(CustomWebApplicationFactory factory)
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
                UserName = "EnrolmentTestUser",
                EmailAddress = email,
                ContactNumber = "0712345678",
                Password = password,
                Role = role
            };

            var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

            registerResponse.EnsureSuccessStatusCode();

            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new
                {
                    EmailAddress = email,
                    Password = password
                });

            loginResponse.EnsureSuccessStatusCode();

            using JsonDocument document = JsonDocument.Parse(await loginResponse.Content.ReadAsStringAsync());

            return document.RootElement.GetProperty("token").GetString()!;
        }

        // Creates an event through the API and returns its ID.
        private async Task<int> CreateEvent(string token)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "/api/events");

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            request.Content = JsonContent.Create(new
            {
                EventName = "Test Running Event",
                Description = "An event created for testing.",
                EventDate = DateTime.UtcNow.AddDays(30),
                Location = "Pretoria",
                Distance = 10.0,
                EventType = "Run"
            });

            var response = await _client.SendAsync(request);

            response.EnsureSuccessStatusCode();

            using JsonDocument document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

            return document.RootElement.GetProperty("eventID").GetInt32();
        }

        // Adds a category to an existing test event.
        private async Task<int> CreateCategory(int eventId)
        {
            using var scope = _factory.Services.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<RaceDayDbContext>();

            var category = new EventCategory
            {
                EventId = eventId,
                CategoryName = "10K",
                Distance = "10",
                AgeRange = "All ages"
            };

            context.EventCategories.Add(category);

            await context.SaveChangesAsync();

            return category.CategoryId;
        }

        // Creates an event and its category for enrolment tests.
        private async Task<(int EventId, int CategoryId)>CreateEventAndCategory()
        {
            string organiserToken = await RegisterAndLogin("Organiser");

            int eventId = await CreateEvent(organiserToken);

            int categoryId = await CreateCategory(eventId);

            return (eventId, categoryId);
        }

        // Sends an enrolment request as a participant.
        private async Task<HttpResponseMessage> Enroll(string token, int eventId, int categoryId)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, $"/api/events/{eventId}/enrolments");

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            request.Content = JsonContent.Create(new
            {
                CategoryId = categoryId
            });

            return await _client.SendAsync(request);
        }

        // Test 1: A participant can enrol in a valid category.
        [Fact]
        public async Task CreateEnrolment_ValidRequest_ReturnsCreated()
        {
            var (eventId, categoryId) = await CreateEventAndCategory();

            string participantToken = await RegisterAndLogin("Participant");

            var response = await Enroll(participantToken, eventId, categoryId);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        // Test 2: Enrolling in a nonexistent event is rejected.
        [Fact]
        public async Task CreateEnrolment_UnknownEvent_ReturnsNotFound()
        {
            string participantToken = await RegisterAndLogin("Participant");

            var response = await Enroll(participantToken, 999999, 1);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // Test 3: A category must belong to the selected event.
        [Fact]
        public async Task CreateEnrolment_WrongCategory_ReturnsNotFound()
        {
            var (eventId, _) = await CreateEventAndCategory();

            string participantToken = await RegisterAndLogin("Participant");

            var response = await Enroll(participantToken, eventId, 999999);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // Test 4: A participant cannot enrol twice in one category.
        [Fact]
        public async Task CreateEnrolment_Duplicate_ReturnsConflict()
        {
            var (eventId, categoryId) = await CreateEventAndCategory();

            string participantToken = await RegisterAndLogin("Participant");

            var firstResponse = await Enroll(participantToken, eventId, categoryId);

            Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

            var secondResponse = await Enroll(participantToken, eventId, categoryId);

            Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
        }

        // Test 5: A participant can retrieve their enrolments.
        [Fact]
        public async Task GetMyEnrolments_Participant_ReturnsOk()
        {
            var (eventId, categoryId) = await CreateEventAndCategory();

            string participantToken = await RegisterAndLogin("Participant");

            var enrolmentResponse = await Enroll(participantToken, eventId, categoryId);

            Assert.Equal(HttpStatusCode.Created, enrolmentResponse.StatusCode);

            using var request = new HttpRequestMessage(HttpMethod.Get, "/api/participants/myEnrolments");

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", participantToken);

            var response = await _client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // Test 6: An organiser cannot use the participant endpoint.
        [Fact]
        public async Task GetMyEnrolments_Organiser_ReturnsForbidden()
        {
            string organiserToken = await RegisterAndLogin("Organiser");

            using var request = new HttpRequestMessage(HttpMethod.Get, "/api/participants/myEnrolments");

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", organiserToken);

            var response = await _client.SendAsync(request);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }
}

