namespace RaceDay.MVC.Models
{
    public class OrganiserDashboardViewModel
    {
        // All events created by the logged-in organiser.
        public List<EventViewModel> Events { get; set; } = new List<EventViewModel>();

        // All enrolments belonging to the organiser's events.
        public List<OrganiserEnrolmentViewModel> Enrolments { get; set; } =
            new List<OrganiserEnrolmentViewModel>();

        // Total number of events created.
        public int TotalEvents { get; set; }

        // Total number of participants enrolled across all events.
        public int TotalEnrolments { get; set; }

        // Number of events that have not yet taken place.
        public int UpcomingEvents { get; set; }
    }
}