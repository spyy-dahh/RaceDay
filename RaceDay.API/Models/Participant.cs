using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models
{
    public class Participant
    {
        [Key]
        public int ParticipantId { get; set; }
        public int UserId { get; set; }
    }
}