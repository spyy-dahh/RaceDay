
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models.DTOs
{
    public class RecordResultDTO
    {
        // The finishing position of the participant
        public int Position { get; set; }

        // The time taken to finish the event
        public TimeSpan FinishTime { get; set; }
    }
}