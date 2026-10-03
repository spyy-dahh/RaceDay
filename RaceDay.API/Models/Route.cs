using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models
{
    public class Route
    {
        [Key]
        public int RouteId { get; set; }
        public int EventId { get; set; }
        public string? RouteDescription { get; set; }
        public decimal Distance { get; set; }
        public string? MapURL { get; set; }
    }
}