namespace WebApi.Auth.MicroServer.Model.Dto
{
    public class RegistrationRequestDto
    {
        public string Name { get; set; }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }

        public string Password { get; set; }
        public string? Role { get; set; }
        public string? SecretKey { get; set; }
    }
}
