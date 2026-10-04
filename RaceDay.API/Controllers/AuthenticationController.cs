using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using RaceDay.API.Data;
using RaceDay.Api.Models;
using RaceDay.Api.Models.DTOs;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthenticationController : ControllerBase
    {
        private readonly RaceDayDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher;

        public AuthenticationController(RaceDayDbContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<User>();
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDTO register)
        {
            // Check that the selected role is valid
            if (register.Role != "Organiser" && register.Role != "Participant")
            {
                return BadRequest(new
                {
                    message = "Role must be Organiser or Participant."
                });
            }

            // Check if email is already registered
            bool emailExists = await _context.Users.AnyAsync(user => user.EmailAddress == register.EmailAddress);

            if (emailExists)
            {
                return Conflict(new
                {
                    message = "Email address is already registered."
                });
            }

            // Create the user
            var user = new User
            {
                UserName = register.UserName,
                EmailAddress = register.EmailAddress,
                ContactNumber = register.ContactNumber,
                Password = ""
            };

            // Hash the password before storing it
            user.Password = _passwordHasher.HashPassword(user, register.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Create the appropriate role record
            if (register.Role == "Organiser")
            {
                var organiser = new EventOrganiser
                {
                    UserId = user.UserId
                };

                _context.EventOrganisers.Add(organiser);
            }
            else
            {
                var participant = new Participant
                {
                    UserId = user.UserId
                };

                _context.Participants.Add(participant);
            }

            await _context.SaveChangesAsync();

            return Created("", new
            {
                message = "Registration successful.",
                userID = user.UserId
            });
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO login)
        {
            // Find the user by email address
            var user = await _context.Users
                .FirstOrDefaultAsync(user => user.EmailAddress == login.EmailAddress);

            if (user == null)
            {
                return Unauthorized(new
                {
                    message = "Invalid email address or password."
                });
            }

            // Check the supplied password against the stored password hash
            var passwordResult = _passwordHasher.VerifyHashedPassword(
                user,
                user.Password,
                login.Password);

            if (passwordResult == PasswordVerificationResult.Failed)
            {
                return Unauthorized(new
                {
                    message = "Invalid email address or password."
                });
            }

            // Determine the user's role
            string? role = null;

            bool isOrganiser = await _context.EventOrganisers
                .AnyAsync(organiser => organiser.UserId == user.UserId);

            if (isOrganiser)
            {
                role = "Organiser";
            }
            else
            {
                bool isParticipant = await _context.Participants
                    .AnyAsync(participant => participant.UserId == user.UserId);

                if (isParticipant)
                {
                    role = "Participant";
                }
            }

            if (role == null)
            {
                return Unauthorized(new
                {
                    message = "User does not have a valid role."
                });
            }

            // Create the JWT
            var claims = new[]
            {
        new System.Security.Claims.Claim(
            System.Security.Claims.ClaimTypes.NameIdentifier,
            user.UserId.ToString()),

        new System.Security.Claims.Claim(
            System.Security.Claims.ClaimTypes.Name,
            user.UserName),

        new System.Security.Claims.Claim(
            System.Security.Claims.ClaimTypes.Role,
            role)
    };

            var key = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(
                    HttpContext.RequestServices
                        .GetRequiredService<IConfiguration>()["Jwt:Key"]!));

            var credentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(
                key,
                Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256);

            var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
                issuer: HttpContext.RequestServices
                    .GetRequiredService<IConfiguration>()["Jwt:Issuer"],

                audience: HttpContext.RequestServices
                    .GetRequiredService<IConfiguration>()["Jwt:Audience"],

                claims: claims,

                expires: DateTime.UtcNow.AddHours(1),

                signingCredentials: credentials
            );

            var tokenString = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler()
                .WriteToken(token);

            return Ok(new
            {
                message = "Login successful.",
                token = tokenString
            });
        }
    }
}