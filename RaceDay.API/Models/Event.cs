using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models
{
    public class Event
    {
        [Key]
        public int EventId { get; set; }
        public int OrganiserId { get; set; }
        public required string EventName { get; set; }
        public string? Description { get; set; }
        public DateTime EventDate { get; set; }
        public required string Location { get; set; }
        public decimal Distance { get; set; }
        public required string EventType { get; set; }

        // Stores the name of the event banner image in Azurite
        public string? ImageName { get; set; }
    }
}