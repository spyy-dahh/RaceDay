
namespace RaceDay.MVC.Models
{
    public class EventViewModel
    {
        // Identifies the event and its organiser.
        public int EventId { get; set; }
        public int OrganiserId { get; set; }

        // Basic information about the event.
        public string EventName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = string.Empty;

        // The distance is decimal because it can contain fractions.
        public decimal Distance { get; set; }

        // For example: Run, Walk or Cycle.
        public string EventType { get; set; } = string.Empty;
    }
}
