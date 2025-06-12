namespace WebApi.Auth.MicroServer.Model.Dto
{
    public class LoginResponseDto
    {
        public UserDto User { get; set; }
        public string Token {  get; set; }  
    }
}
