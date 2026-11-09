using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Models.DTOs;
using RaceDay.API.Data;
using RaceDay.API.Services;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserProfileController : ControllerBase
    {
        private readonly RaceDayDbContext _context;
        private readonly BlobStorageService _blobStorageService;

        public UserProfileController(RaceDayDbContext context, BlobStorageService blobStorageService)
        {
            _context = context;
            _blobStorageService = blobStorageService;
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            // Get the UserId from the JWT
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return Unauthorized(new
                {
                    message = "User is not authenticated."
                });
            }

            int userId = int.Parse(userIdClaim.Value);

            // Find the user in the database
            var user = await _context.Users.FirstOrDefaultAsync(user => user.UserId == userId);

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

        [Authorize]
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile(UpdateProfileDTO updateProfile)
        {
            // Get the user ID from the JWT token
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);

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

            // Check that the contact number was provided
            if (string.IsNullOrWhiteSpace(updateProfile.ContactNumber))
            {
                return BadRequest(new
                {
                    message = "Contact number is required."
                });
            }

            // Update the contact number
            user.ContactNumber = updateProfile.ContactNumber;

            await _context.SaveChangesAsync();

            // Return the updated profile
            return Ok(new
            {
                message = "Profile updated successfully.",
                userID = user.UserId,
                userName = user.UserName,
                emailAddress = user.EmailAddress,
                contactNumber = user.ContactNumber
            });
        }


        // Delete the logged-in user's account
        [Authorize]
        [HttpDelete("profile")]
        public async Task<IActionResult> DeleteProfile()
        {
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

            var user = await _context.Users
                .FirstOrDefaultAsync(user => user.UserId == userId);

            if (user == null)
            {
                return NotFound(new
                {
                    message = "User profile not found."
                });
            }

            // Check if the user is an organiser
            var organiser = await _context.EventOrganisers
                .FirstOrDefaultAsync(
                    organiser => organiser.UserId == userId);

            // Check if the user is a participant
            var participant = await _context.Participants
                .FirstOrDefaultAsync(
                    participant => participant.UserId == userId);

            // Remove the role record first
            if (organiser != null)
            {
                _context.EventOrganisers.Remove(organiser);
            }

            if (participant != null)
            {
                _context.Participants.Remove(participant);
            }

            // Remove the user account
            _context.Users.Remove(user);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Account deleted."
            });
        }

        [Authorize(Roles = "Participant")]
        [HttpPost("profile/image")]
        public async Task<IActionResult> UploadProfileImage(IFormFile image)
        {
            // Get the logged-in user's ID from the JWT token
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

            // Find the user
            var user = await _context.Users
                .FirstOrDefaultAsync(user => user.UserId == userId);

            if (user == null)
            {
                return NotFound(new
                {
                    message = "User not found."
                });
            }

            // Remember the previous profile image
            string? oldImageName = user.ProfileImageName;

            try
            {
                // Upload the new image to Blob Storage
                string newImageName = await _blobStorageService.UploadImageAsync(image);

                // Save the new image filename
                user.ProfileImageName = newImageName;

                await _context.SaveChangesAsync();

                // Delete the previous image if one existed
                if (!string.IsNullOrEmpty(oldImageName))
                {
                    await _blobStorageService.DeleteImageAsync(oldImageName);
                }

                return Ok(new
                {
                    message = "Profile picture uploaded successfully.",
                    imageName = newImageName
                });
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new
                {
                    message = exception.Message
                });
            }
        }

        [Authorize]
        [HttpGet("profile/image")]
        public async Task<IActionResult> GetProfileImage()
        {
            // Get the logged-in user's ID from the JWT token
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

            // Find the user
            var user = await _context.Users
                .FirstOrDefaultAsync(user => user.UserId == userId);

            if (user == null)
            {
                return NotFound(new
                {
                    message = "User not found."
                });
            }

            // Check whether the user has uploaded a profile picture
            if (string.IsNullOrEmpty(user.ProfileImageName))
            {
                return NotFound(new
                {
                    message = "No profile picture has been uploaded."
                });
            }

            // Retrieve the picture from Blob Storage
            var image = await _blobStorageService.GetImageAsync(
                user.ProfileImageName);

            if (image == null)
            {
                return NotFound(new
                {
                    message = "Profile picture not found."
                });
            }

            // Return the image to the browser
            return File(
                image.Content,
                image.Details.ContentType ?? "application/octet-stream"
            );
        }
    }
}