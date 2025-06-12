using WebApi.Auth.MicroServer.Model;

namespace WebApi.Auth.MicroServer.Service.IService
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(ApplicationUser applicationUser, IEnumerable<string> roles);
    }
}
