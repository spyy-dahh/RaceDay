using System.ComponentModel.DataAnnotations;

namespace RaceDay.MVC.Models
{
    public class CreateEventViewModel
    {
        [Required(ErrorMessage = "Please enter an event name.")]
        public string EventName { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required(ErrorMessage = "Please select an event date.")]
        public DateTime EventDate { get; set; }

        [Required(ErrorMessage = "Please enter an event location.")]
        public string Location { get; set; } = string.Empty;

        [Range(0.01, 1000, ErrorMessage = "Distance must be greater than zero.")]
        public decimal Distance { get; set; }

        [Required(ErrorMessage = "Please select an event type.")]
        public string EventType { get; set; } = string.Empty;
    }
}