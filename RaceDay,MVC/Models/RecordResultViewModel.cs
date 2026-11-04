using System.ComponentModel.DataAnnotations;

namespace RaceDay.MVC.Models
{
    public class RecordResultViewModel
    {
        public int EnrolmentId { get; set; }

        public string ParticipantName { get; set; } = string.Empty;

        public string EventName { get; set; } = string.Empty;

        public string CategoryName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter the finishing position.")]
        [Range(1, int.MaxValue, ErrorMessage = "Position must be at least 1.")]
        public int Position { get; set; }

        [Required(ErrorMessage = "Please enter the finishing time.")]
        public string FinishTime { get; set; } = string.Empty;
    }
}