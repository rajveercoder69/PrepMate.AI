using WebApi.Auth.MicroServer.Model;
using WebApi.Auth.MicroServer.Model.Dto;

namespace WebApi.Auth.MicroServer.Service.IService
{
    public interface IAuthService
    {
        Task<string> Regestiration(RegistrationRequestDto registrationRequestDto);
        Task<LoginResponseDto> Login(LoginRequestDto loginRequestDto);
        Task<bool> assignRole(string email,string roleName);
        Task<string> CreateRolePermissionRequest(RolePermission model);
        Task<PermissionResponseDto> GetPendingPermissionList(string userEmail);
        Task<PermissionResponseDto> UpsertPermission(RolePermissionUpdateDto rolePermissionUpdateDto,string adminUser);

    }
}
