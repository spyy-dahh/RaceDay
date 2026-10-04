namespace RaceDay.Api.Models.DTOs
{
    public class UpdateEventDTO
    {
        public required string EventName { get; set; }
        public string? Description { get; set; }
        public DateTime EventDate { get; set; }
        public required string Location { get; set; }
        public decimal Distance { get; set; }
        public required string EventType { get; set; }
    }
}