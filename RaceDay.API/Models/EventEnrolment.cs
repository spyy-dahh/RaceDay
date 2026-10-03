using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models
{
    public class EventEnrolment
    {
        [Key]
        public int EnrolmentId { get; set; }
        public int EventId { get; set; }
        public int CategoryId { get; set; }
        public int ParticipantId { get; set; }
        public DateTime EnrolmentDate { get; set; }
        public string Status { get; set; } = "Registered";
    }
}