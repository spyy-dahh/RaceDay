using System.ComponentModel.DataAnnotations;


namespace RaceDay.Api.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }
        public required string UserName { get; set; }
        public required string EmailAddress { get; set; }
        public required string ContactNumber { get; set; }
        public required string Password { get; set; }
    }
}