using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.Api.Models;
using RaceDay.Api.Models.DTOs;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api/events")]
    public class EventsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EventsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // Get all events
        [HttpGet]
        public async Task<IActionResult> GetEvents()
        {
            var events = await _context.Events.ToListAsync();

            return Ok(events);
        }

        // Get one event by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEvent(int id)
        {
            var eventItem = await _context.Events.FirstOrDefaultAsync(eventItem => eventItem.EventId == id);

            if (eventItem == null)
            {
                return NotFound(new
                {
                    message = "Event not found."
                });
            }

            return Ok(eventItem);
        }

        // Create a new event
        [Authorize(Roles = "Organiser")]
        [HttpPost]
        public async Task<IActionResult> CreateEvent(CreateEventDTO createEvent)
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
            // Find the organiser linked to the logged-in user
            var organiser = await _context.EventOrganisers.FirstOrDefaultAsync(organiser => organiser.UserId == userId);

            if (organiser == null)
            {
                return NotFound(new
                {
                    message = "Organiser profile not found."
                });
            }

            // Check the event type
            if (createEvent.EventType != "Run" && createEvent.EventType != "Walk" && createEvent.EventType != "Cycle")
            {
                return BadRequest(new
                {
                    message = "Event type must be Run, Walk, or Cycle."
                });
            }

            // Check the event distance
            if (createEvent.Distance <= 0)
            {
                return BadRequest(new
                {
                    message = "Event distance must be greater than 0."
                });
            }

            // Create the event
            var eventItem = new Event
            {
                OrganiserId = organiser.OrganiserId,
                EventName = createEvent.EventName,
                Description = createEvent.Description,
                EventDate = createEvent.EventDate,
                Location = createEvent.Location,
                Distance = createEvent.Distance,
                EventType = createEvent.EventType
            };

            _context.Events.Add(eventItem);

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEvent),new 
            { id = eventItem.EventId },new
            {
                eventID = eventItem.EventId,
                message = "Event created successfully."
            });
        }

        // Update an event
        [Authorize(Roles = "Organiser")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEvent(int id, UpdateEventDTO updateEvent)
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

            // Find the event
            var eventItem = await _context.Events.FirstOrDefaultAsync(eventItem => eventItem.EventId == id);

            if (eventItem == null)
            {
                return NotFound(new
                {
                    message = "Event not found."
                });
            }

            // Check that the organiser owns the event
            if (eventItem.OrganiserId != organiser.OrganiserId)
            {
                return Forbid();
            }

            // Check the event type
            if (updateEvent.EventType != "Run" && updateEvent.EventType != "Walk" && updateEvent.EventType != "Cycle")
            {
                return BadRequest(new
                {
                    message = "Event type must be Run, Walk, or Cycle."
                });
            }

            // Check the event distance
            if (updateEvent.Distance <= 0)
            {
                return BadRequest(new
                {
                    message = "Event distance must be greater than 0."
                });
            }

            // Update the event
            eventItem.EventName = updateEvent.EventName;
            eventItem.Description = updateEvent.Description;
            eventItem.EventDate = updateEvent.EventDate;
            eventItem.Location = updateEvent.Location;
            eventItem.Distance = updateEvent.Distance;
            eventItem.EventType = updateEvent.EventType;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Event updated successfully.",
                eventID = eventItem.EventId
            });
        }

        // Delete an event
        [Authorize(Roles = "Organiser")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
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

            // Find the event
            var eventItem = await _context.Events.FirstOrDefaultAsync(eventItem => eventItem.EventId == id);

            if (eventItem == null)
            {
                return NotFound(new
                {
                    message = "Event not found."
                });
            }

            // Check that the organiser owns the event
            if (eventItem.OrganiserId != organiser.OrganiserId)
            {
                return Forbid();
            }

            // Check if the event has categories
            bool hasCategories = await _context.EventCategories.AnyAsync(category => category.EventId == id);

            // Check if the event has enrolments
            bool hasEnrolments = await _context.EventEnrolments.AnyAsync(enrolment => enrolment.EventId == id);

            // Check if the event has a route
            bool hasRoute = await _context.Routes.AnyAsync(route => route.EventId == id);

            if (hasCategories || hasEnrolments || hasRoute)
            {
                return Conflict(new
                {
                    message = "Event cannot be deleted because it has related data."
                });
            }

            _context.Events.Remove(eventItem);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Event deleted successfully."
            });
        }

        // Get events created by the logged-in organiser
        [Authorize(Roles = "Organiser")]
        [HttpGet("/api/organisers/events")]
        public async Task<IActionResult> GetOrganiserEvents()
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

            // Get events belonging to the organiser
            var events = await _context.Events.Where(eventItem =>eventItem.OrganiserId == organiser.OrganiserId).ToListAsync();

            return Ok(events);
        }
    }
}