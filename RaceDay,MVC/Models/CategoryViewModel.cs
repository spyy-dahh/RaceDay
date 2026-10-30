using System.ComponentModel.DataAnnotations;

namespace RaceDay.MVC.Models
{
    public class CategoryViewModel
    {
        public int CategoryId { get; set; }

        public int EventId { get; set; }

        [Required(ErrorMessage = "Please enter a category name.")]
        public string CategoryName { get; set; } = string.Empty;

        public string? Distance { get; set; }

        public string? AgeRange { get; set; }
    }
}