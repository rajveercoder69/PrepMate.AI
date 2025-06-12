using Microsoft.AspNetCore.Identity;

namespace WebApi.Auth.MicroServer.Model
{
    public class ApplicationUser:IdentityUser
    {
        public string Name { get; set; }
    }
}
