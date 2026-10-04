namespace RaceDay.Api.Models.DTOs
{
    public class LoginDTO
    {
        public required string EmailAddress { get; set; }
        public required string Password { get; set; }
    }
}