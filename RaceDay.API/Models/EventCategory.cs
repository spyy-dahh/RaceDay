namespace RaceDay.Api.Models
{
    public class EventCategory
    {
        public int CategoryId { get; set; }
        public int EventId { get; set; }
        public required string CategoryName { get; set; }
        public string? Distance { get; set; }
        public string? AgeRange { get; set; }
    }
}