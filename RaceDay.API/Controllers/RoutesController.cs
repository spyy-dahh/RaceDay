using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api/events")]
    public class RoutesController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public RoutesController(RaceDayDbContext context)
        {
            _context = context;
        }

        // Get the route details for an event
        [HttpGet("{id}/route")]
        public async Task<IActionResult> GetEventRoute(int id)
        {
            // Check whether the event exists
            var eventItem = await _context.Events.FirstOrDefaultAsync(eventItem => eventItem.EventId == id);

            if (eventItem == null)
            {
                return NotFound(new
                {
                    message = "Event not found."
                });
            }

            // Find the route belonging to this event
            var route = await _context.Routes.FirstOrDefaultAsync(route => route.EventId == id);

            if (route == null)
            {
                return NotFound(new
                {
                    message = "Route not found for this event."
                });
            }

            // Return the route details
            return Ok(new
            {
                routeID = route.RouteId,
                eventID = route.EventId,
                routeDescription = route.RouteDescription,
                distance = route.Distance,
                mapURL = route.MapURL
            });
        }
    }
}