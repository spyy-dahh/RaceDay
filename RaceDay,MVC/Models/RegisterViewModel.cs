
using System.ComponentModel.DataAnnotations;

namespace RaceDay.MVC.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Please enter your username.")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your email address.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string EmailAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your contact number.")]
        public string ContactNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter a password.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select your account type.")]
        public string Role { get; set; } = string.Empty;
    }
}
