using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserProfileController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public UserProfileController(RaceDayDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            // Get the UserId from the JWT
            var userIdClaim = User.FindFirst(
                System.Security.Claims.ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return Unauthorized(new
                {
                    message = "User is not authenticated."
                });
            }

            int userId = int.Parse(userIdClaim.Value);

            // Find the user in the database
            var user = await _context.Users
                .FirstOrDefaultAsync(user => user.UserId == userId);

            if (user == null)
            {
                return NotFound(new
                {
                    message = "User profile not found."
                });
            }

            // Do not return the password
            return Ok(new
            {
                userID = user.UserId,
                userName = user.UserName,
                emailAddress = user.EmailAddress,
                contactNumber = user.ContactNumber
            });
        }
    }
}