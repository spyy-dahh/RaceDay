namespace RaceDay.MVC.Models
{
    public class MyResultViewModel
    {
        public int ResultId { get; set; }

        public int EventId { get; set; }

        public string EventName { get; set; } = string.Empty;

        public string CategoryName { get; set; } = string.Empty;

        public int Position { get; set; }

        public string FinishTime { get; set; } = string.Empty;

        public DateTime DateOfResults { get; set; }
    }
}