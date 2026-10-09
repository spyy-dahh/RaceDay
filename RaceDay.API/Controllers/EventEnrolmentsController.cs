
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.Api.Models;
using RaceDay.Api.Models.DTOs;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api")]
    public class EventEnrolmentsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EventEnrolmentsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // A participant enrols in an event category
        [Authorize(Roles = "Participant")]
        [HttpPost("events/{id}/enrolments")]
        public async Task<IActionResult> CreateEnrolment(
            int id, CreateEnrolmentDTO createEnrolment)
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

            // Find the participant linked to the logged-in user
            var participant = await _context.Participants.FirstOrDefaultAsync(participant => participant.UserId == userId);

            if (participant == null)
            {
                return NotFound(new
                {
                    message = "Participant profile not found."
                });
            }

            // Check that the event exists
            var eventItem = await _context.Events.FirstOrDefaultAsync(eventItem => eventItem.EventId == id);

            if (eventItem == null)
            {
                return NotFound(new
                {
                    message = "Event not found."
                });
            }

            // Check that the selected category belongs to this event
            var category = await _context.EventCategories.FirstOrDefaultAsync(category => category.CategoryId == createEnrolment.CategoryId && category.EventId == id);

            if (category == null)
            {
                return NotFound(new
                {
                    message = "Category not found for this event."
                });
            }

            // Check whether the participant already enrolled in this category
            bool enrolmentExists = await _context.EventEnrolments.AnyAsync(enrolment => enrolment.ParticipantId == participant.ParticipantId && enrolment.EventId == id && enrolment.CategoryId == createEnrolment.CategoryId);

            if (enrolmentExists)
            {
                return Conflict(new
                {
                    message = "You have already enrolled in this category."
                });
            }

            // Create the enrolment
            var enrolmentItem = new EventEnrolment
            {
                EventId = id,
                CategoryId = createEnrolment.CategoryId,
                ParticipantId = participant.ParticipantId,
                EnrolmentDate = DateTime.Today,
                Status = "Registered"
            };

            _context.EventEnrolments.Add(enrolmentItem);

            await _context.SaveChangesAsync();

            return Created("", new
            {
                enrolmentID = enrolmentItem.EnrolmentId,
                eventID = enrolmentItem.EventId,
                categoryID = enrolmentItem.CategoryId,
                participantID = enrolmentItem.ParticipantId,
                enrolmentDate = enrolmentItem.EnrolmentDate,
                status = enrolmentItem.Status,
                message = "Enrolment created successfully."
            });
        }

        // Get enrolments belonging to the logged-in participant
        [Authorize(Roles = "Participant")]
        [HttpGet("participants/myEnrolments")]
        public async Task<IActionResult> GetMyEnrolments()
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

            // Find the participant
            var participant = await _context.Participants.FirstOrDefaultAsync(participant => participant.UserId == userId);

            if (participant == null)
            {
                return NotFound(new
                {
                    message = "Participant profile not found."
                });
            }

            // Get the participant's enrolments with event and category details
            var enrolments = await (
                from enrolment in _context.EventEnrolments
                join eventItem in _context.Events
                    on enrolment.EventId equals eventItem.EventId
                join category in _context.EventCategories
                    on enrolment.CategoryId equals category.CategoryId
                where enrolment.ParticipantId == participant.ParticipantId
                select new
                {
                    enrolmentID = enrolment.EnrolmentId,
                    eventID = eventItem.EventId,
                    eventName = eventItem.EventName,
                    categoryID = category.CategoryId,
                    categoryName = category.CategoryName,
                    enrolmentDate = enrolment.EnrolmentDate,
                    status = enrolment.Status
                }
            ).ToListAsync();

            return Ok(enrolments);
        }

        // Get enrolments for events belonging to the logged-in organiser
        [Authorize(Roles = "Organiser")]
        [HttpGet("organisers/enrolments")]
        public async Task<IActionResult> GetOrganiserEnrolments()
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

            // Find the organiser
            var organiser = await _context.EventOrganisers.FirstOrDefaultAsync(organiser => organiser.UserId == userId);

            if (organiser == null)
            {
                return NotFound(new
                {
                    message = "Organiser profile not found."
                });
            }

            // Get enrolments for this organiser's events only
            var enrolments = await (
                from enrolment in _context.EventEnrolments
                join eventItem in _context.Events
                    on enrolment.EventId equals eventItem.EventId
                join category in _context.EventCategories
                    on enrolment.CategoryId equals category.CategoryId
                join participant in _context.Participants
                    on enrolment.ParticipantId equals participant.ParticipantId
                join user in _context.Users
                    on participant.UserId equals user.UserId
                where eventItem.OrganiserId == organiser.OrganiserId
                select new
                {
                    enrolmentID = enrolment.EnrolmentId,
                    eventID = eventItem.EventId,
                    eventName = eventItem.EventName,
                    categoryID = category.CategoryId,
                    categoryName = category.CategoryName,
                    participantID = participant.ParticipantId,
                    participantName = user.UserName,
                    enrolmentDate = enrolment.EnrolmentDate,
                    status = enrolment.Status
                }
            ).ToListAsync();

            return Ok(enrolments);
        }
    }
}