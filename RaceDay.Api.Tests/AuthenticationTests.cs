using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace RaceDay.Api.Tests
{
    public class AuthenticationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public AuthenticationTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        // Creates a new test user with a unique email address.
        private static object CreateRegisterRequest(string email, string role = "Participant")
        {
            return new
            {
                UserName = "TestUser",
                EmailAddress = email,
                ContactNumber = "0712345678",
                Password = "TestPassword123!",
                Role = role
            };
        }

        // Test 1: A participant can register successfully. (201 Created)
        [Fact]
        public async Task Register_ValidParticipant_ReturnsCreated()
        {
            string email = $"participant-{Guid.NewGuid():N}@gmail.com";
            
            var request = CreateRegisterRequest(email);

            var response = await _client.PostAsJsonAsync("/api/auth/register", request);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        // Test 2: An invalid role is rejected. (400 bad request)
        [Fact]
        public async Task Register_InvalidRole_ReturnsBadRequest()
        {
            string email = $"invalid-{Guid.NewGuid():N}@gmail.com";

            var request = CreateRegisterRequest(email, "Administrator");

            var response = await _client.PostAsJsonAsync("/api/auth/register", request);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // Test 3: The same email cannot register twice. (409 Conflict)
        [Fact]
        public async Task Register_DuplicateEmail_ReturnsConflict()
        {
            string email = $"duplicate-{Guid.NewGuid():N}@gmail.com";

            var request = CreateRegisterRequest(email);

            await _client.PostAsJsonAsync("/api/auth/register", request);

            var secondResponse = await _client.PostAsJsonAsync("/api/auth/register", request);

            Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
        }

        // Test 4: A registered user can log in. (200 ok)
        public async Task Login_ValidCredentials_ReturnsToken()
        {
            string email = $"login-{Guid.NewGuid():N}@gmail.com";
            string password = "TestPassword123!";

            var registerRequest = new
            {
                UserName = "LoginUser",
                EmailAddress = email,
                ContactNumber = "0712345678",
                Password = password,
                Role = "Participant"
            };

            await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

            var loginRequest = new
            {
                EmailAddress = email,
                Password = password
            };

            var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            string responseBody =await response.Content.ReadAsStringAsync();

            using JsonDocument document = JsonDocument.Parse(responseBody);

            bool tokenExists = document.RootElement.TryGetProperty("token", out JsonElement token);

            Assert.True(tokenExists);
            Assert.False(string.IsNullOrWhiteSpace(token.GetString()));
        }

        // Test 5: An incorrect password is rejected. (401 Unauthorized)
        [Fact]
        public async Task Login_IncorrectPassword_ReturnsUnauthorized()
        {
            string email = $"wrongpass-{Guid.NewGuid():N}@gmail.com";

            var registerRequest = CreateRegisterRequest(email);

            await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

            var loginRequest = new
            {
                EmailAddress = email,
                Password = "WrongPassword123!"
            };

            var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        // Test 6: An unknown email address is rejected. (401 Unauthorized)
        [Fact]
        public async Task Login_UnknownEmail_ReturnsUnauthorized()
        {
            var loginRequest = new
            {
                EmailAddress = $"unknown-{Guid.NewGuid():N}@gmail.com",
                Password = "TestPassword123!"
            };

            var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}