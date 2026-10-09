
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.Api.Models;
using RaceDay.Api.Models.DTOs;
using System.Security.Claims;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api")]
    public class ResultsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public ResultsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // An organiser records a participant's result
        [Authorize(Roles = "Organiser")]
        [HttpPost("enrolments/{id}/results")]
        public async Task<IActionResult> RecordResult(int id, RecordResultDTO recordResult)
        {
            // Get the logged-in user's ID from the JWT token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return Unauthorized(new
                {
                    message = "User is not authenticated."
                });
            }

            int userId = int.Parse(userIdClaim.Value);

            // Find the organiser profile
            var organiser = await _context.EventOrganisers.FirstOrDefaultAsync(organiser => organiser.UserId == userId);

            if (organiser == null)
            {
                return NotFound(new
                {
                    message = "Organiser profile not found."
                });
            }

            // Find the enrolment and its event
            var enrolment = await _context.EventEnrolments.FirstOrDefaultAsync(enrolment => enrolment.EnrolmentId == id);

            if (enrolment == null)
            {
                return NotFound(new
                {
                    message = "Enrolment not found."
                });
            }

            var eventItem = await _context.Events.FirstOrDefaultAsync(eventItem => eventItem.EventId == enrolment.EventId);

            if (eventItem == null)
            {
                return NotFound(new
                {
                    message = "Event not found."
                });
            }

            // Make sure this organiser owns the event
            if (eventItem.OrganiserId != organiser.OrganiserId)
            {
                return Forbid();
            }

            // Check that the submitted result is valid
            if (recordResult.Position < 1)
            {
                return BadRequest(new
                {
                    message = "Position must be greater than zero."
                });
            }

            if (recordResult.FinishTime <= TimeSpan.Zero)
            {
                return BadRequest(new
                {
                    message = "Finish time must be greater than zero."
                });
            }

            // Check whether a result already exists for this enrolment
            bool resultExists = await _context.Results.AnyAsync(result => result.EnrolmentId == id);

            if (resultExists)
            {
                return Conflict(new
                {
                    message = "A result already exists for this enrolment."
                });
            }

            // Create the result
            var resultItem = new Result
            {
                EnrolmentId = id,
                Position = recordResult.Position,
                FinishTime = recordResult.FinishTime,
                DateOfResults = DateTime.Today
            };

            _context.Results.Add(resultItem);

            await _context.SaveChangesAsync();

            return Created("", new
            {
                resultID = resultItem.ResultId,
                enrolmentID = resultItem.EnrolmentId,
                position = resultItem.Position,
                finishTime = resultItem.FinishTime,
                dateOfResults = resultItem.DateOfResults,
                message = "Result recorded successfully."
            });
        }

        // A participant views their own results
        [Authorize(Roles = "Participant")]
        [HttpGet("results/myResults")]
        public async Task<IActionResult> GetMyResults()
        {
            // Get the logged-in user's ID from the JWT token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return Unauthorized(new
                {
                    message = "User is not authenticated."
                });
            }

            int userId = int.Parse(userIdClaim.Value);

            // Find the participant profile
            var participant = await _context.Participants.FirstOrDefaultAsync(participant => participant.UserId == userId);

            if (participant == null)
            {
                return NotFound(new
                {
                    message = "Participant profile not found."
                });
            }

            // Retrieve results linked to this participant's enrolments
            var results = await (
                from result in _context.Results
                join enrolment in _context.EventEnrolments
                    on result.EnrolmentId equals enrolment.EnrolmentId
                join eventItem in _context.Events
                    on enrolment.EventId equals eventItem.EventId
                join category in _context.EventCategories
                    on enrolment.CategoryId equals category.CategoryId
                where enrolment.ParticipantId == participant.ParticipantId
                select new
                {
                    resultID = result.ResultId,
                    eventID = eventItem.EventId,
                    eventName = eventItem.EventName,
                    categoryName = category.CategoryName,
                    position = result.Position,
                    finishTime = result.FinishTime,
                    dateOfResults = result.DateOfResults
                }
            ).ToListAsync();

            return Ok(results);
        }
    }
}