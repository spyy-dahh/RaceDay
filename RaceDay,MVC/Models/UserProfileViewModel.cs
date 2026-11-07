using System.ComponentModel.DataAnnotations;

namespace RaceDay.MVC.Models
{
    public class UserProfileViewModel
    {
        public int UserId { get; set; }

        public string UserName { get; set; } = string.Empty;

        public string EmailAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your contact number.")]
        [Phone(ErrorMessage = "Please enter a valid contact number.")]
        public string ContactNumber { get; set; } = string.Empty;
    }
}