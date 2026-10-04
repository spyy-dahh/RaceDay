namespace RaceDay.Api.Models.DTOs
{
    public class RegisterDTO
    {
        public required string UserName { get; set; }
        public required string EmailAddress { get; set; }
        public required string ContactNumber { get; set; }
        public required string Password { get; set; }
        public required string Role { get; set; }//will help with determining parti. or orga. - its not a db column, contolller will use it do differentiate
    }
}