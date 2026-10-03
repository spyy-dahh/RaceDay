using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models
{
    public class EventOrganiser
    {
        [Key]
        public int OrganiserId { get; set; }
        public int UserId { get; set; }
    }
}