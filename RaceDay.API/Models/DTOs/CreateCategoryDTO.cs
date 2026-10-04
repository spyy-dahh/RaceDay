namespace RaceDay.Api.Models.DTOs
{
    public class CreateCategoryDTO
    {
        public required string CategoryName { get; set; }
        public string? Distance { get; set; }
        public string? AgeRange { get; set; }
    }
}