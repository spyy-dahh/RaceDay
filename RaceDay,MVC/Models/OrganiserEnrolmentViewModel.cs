namespace RaceDay.MVC.Models
{
    public class OrganiserEnrolmentViewModel
    {
        public int EnrolmentId { get; set; }

        public int EventId { get; set; }

        public string EventName { get; set; } = string.Empty;

        public int CategoryId { get; set; }

        public string CategoryName { get; set; } = string.Empty;

        public int ParticipantId { get; set; }

        public string ParticipantName { get; set; } = string.Empty;

        public DateTime EnrolmentDate { get; set; }

        public string Status { get; set; } = string.Empty;
    }
}