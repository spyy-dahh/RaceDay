using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.Api.Models;
using RaceDay.Api.Models.DTOs;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public CategoriesController(RaceDayDbContext context)
        {
            _context = context;
        }

        // Get all categories for an event
        [HttpGet("api/events/{id}/categories")]
        public async Task<IActionResult> GetCategories(int id)
        {
            // Check that the event exists
            var eventExists = await _context.Events.AnyAsync(eventItem => eventItem.EventId == id);

            if (!eventExists)
            {
                return NotFound(new
                {
                    message = "Event not found."
                });
            }

            var categories = await _context.EventCategories.Where(category => category.EventId == id).ToListAsync();

            return Ok(categories);
        }

        // Create a category for an event
        [Authorize(Roles = "Organiser")]
        [HttpPost("api/events/{id}/categories")]
        public async Task<IActionResult> CreateCategory(int id, CreateCategoryDTO createCategory)
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

            // Check if the category already exists for this event
            bool categoryExists = await _context.EventCategories.AnyAsync(category => category.EventId == id && category.CategoryName == createCategory.CategoryName);

            if (categoryExists)
            {
                return Conflict(new
                {
                    message = "A category with this name already exists for this event."
                });
            }

            // Create the category
            var categoryItem = new EventCategory
            {
                EventId = id,
                CategoryName = createCategory.CategoryName,
                Distance = createCategory.Distance,
                AgeRange = createCategory.AgeRange
            };

            _context.EventCategories.Add(categoryItem);

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCategories), new { id = id },
                new
                {
                    categoryID = categoryItem.CategoryId,
                    message = "Category created successfully."
                });
        }

        // Update a category
        [Authorize(Roles = "Organiser")]
        [HttpPut("api/categories/{id}")]
        public async Task<IActionResult> UpdateCategory(int id, UpdateCategoryDTO updateCategory)
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

            // Find the category
            var categoryItem = await _context.EventCategories.FirstOrDefaultAsync(category => category.CategoryId == id);

            if (categoryItem == null)
            {
                return NotFound(new
                {
                    message = "Category not found."
                });
            }

            // Find the event belonging to the category
            var eventItem = await _context.Events.FirstOrDefaultAsync(eventItem => eventItem.EventId == categoryItem.EventId);

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

            // Check if another category already uses this name
            bool categoryExists = await _context.EventCategories.AnyAsync(category => category.EventId == categoryItem.EventId && category.CategoryName == updateCategory.CategoryName && category.CategoryId != id);

            if (categoryExists)
            {
                return Conflict(new
                {
                    message = "A category with this name already exists for this event."
                });
            }

            // Update the category
            categoryItem.CategoryName = updateCategory.CategoryName;
            categoryItem.Distance = updateCategory.Distance;
            categoryItem.AgeRange = updateCategory.AgeRange;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Category updated successfully.",
                categoryID = categoryItem.CategoryId
            });
        }

        // Delete a category
        [Authorize(Roles = "Organiser")]
        [HttpDelete("api/categories/{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
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

            // Find the category
            var categoryItem = await _context.EventCategories.FirstOrDefaultAsync(category => category.CategoryId == id);

            if (categoryItem == null)
            {
                return NotFound(new
                {
                    message = "Category not found."
                });
            }

            // Find the event belonging to the category
            var eventItem = await _context.Events.FirstOrDefaultAsync(eventItem => eventItem.EventId == categoryItem.EventId);

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

            // Check if the category is being used by an enrolment
            bool hasEnrolments = await _context.EventEnrolments.AnyAsync(enrolment => enrolment.CategoryId == id);

            if (hasEnrolments)
            {
                return Conflict(new
                {
                    message = "Category cannot be deleted because it is being used by an enrolment."
                });
            }

            _context.EventCategories.Remove(categoryItem);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Category deleted successfully."
            });
        }
    }
}
