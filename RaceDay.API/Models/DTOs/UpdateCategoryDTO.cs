namespace RaceDay.Api.Models.DTOs
{
    public class UpdateCategoryDTO
    {
        public required string CategoryName { get; set; }
        public string? Distance { get; set; }
        public string? AgeRange { get; set; }
    }
}