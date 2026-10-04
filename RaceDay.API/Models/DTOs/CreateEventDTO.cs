namespace RaceDay.Api.Models.DTOs
{
    public class CreateEventDTO
    {
        //organiseriD wll come from the token so that the url cant be edited
        public required string EventName { get; set; }
        public string? Description { get; set; }
        public DateTime EventDate { get; set; }
        public required string Location { get; set; }
        public decimal Distance { get; set; }
        public required string EventType { get; set; }
    }
}