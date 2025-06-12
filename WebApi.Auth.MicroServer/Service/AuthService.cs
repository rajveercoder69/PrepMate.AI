using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using WebApi.Auth.MicroServer.Model;
using WebApi.Auth.MicroServer.Model.Dto;
using WebApi.Auth.MicroServer.Service.IService;

namespace WebApi.Auth.MicroServer.Service
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly PermissionResponseDto _permissionResponseDto;
        
        public AuthService(ApplicationDbContext db, IJwtTokenGenerator jwtTokenGenerator,
            UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _db = db;
            _jwtTokenGenerator = jwtTokenGenerator;
            _userManager = userManager;
            _roleManager = roleManager;
            _permissionResponseDto = new();
           
           

        }
        public async Task<bool> assignRole(string email, string roleName)
        {
            var user = _db.ApplicationUsers.FirstOrDefault(x => x.Email.ToLower() == email.ToLower());
            if (user != null)
            {
                if (!_roleManager.RoleExistsAsync(roleName).GetAwaiter().GetResult())
                {
                    //create role if it does not exist
                    _roleManager.CreateAsync(new IdentityRole(roleName)).GetAwaiter().GetResult();
                }
                await _userManager.AddToRoleAsync(user, roleName);
                return true;
            }
            return false;
        }

        public async Task<LoginResponseDto> Login(LoginRequestDto loginRequestDto)
        {
            var user = _db.ApplicationUsers.FirstOrDefault(u => u.UserName.ToLower() == loginRequestDto.UserName.ToLower());

            bool isValid = await _userManager.CheckPasswordAsync(user, loginRequestDto.Password);

            if (user == null || isValid == false)
            {
                return new LoginResponseDto() { User = null, Token = "" };
            }

            //if user was found , Generate JWT Token
            var roles = await _userManager.GetRolesAsync(user);
            var token = _jwtTokenGenerator.GenerateToken(user, roles);

            UserDto userDTO = new()
            {
                Email = user.Email,
                ID = user.Id,
                Name = user.Name,
                PhoneNumber = user.PhoneNumber
            };

            LoginResponseDto loginResponseDto = new LoginResponseDto()
            {
                User = userDTO,
                Token = token
            };

            return loginResponseDto;
        }

        public async Task<string> Regestiration(RegistrationRequestDto registrationRequestDto)
        {
            ApplicationUser user = new()
            {
                UserName = registrationRequestDto.Email,
                Email = registrationRequestDto.Email,
                NormalizedEmail = registrationRequestDto.Email.ToUpper(),
                Name = registrationRequestDto.Name,
                PhoneNumber = registrationRequestDto.PhoneNumber
            };
            try
            {
                var result = await _userManager.CreateAsync(user, registrationRequestDto.Password);
                if (result.Succeeded)
                {
                    var userToReturn = _db.ApplicationUsers.First(u => u.UserName == registrationRequestDto.Email);

                    UserDto userDto = new()
                    {
                        Email = userToReturn.Email,
                        ID = userToReturn.Id,
                        Name = userToReturn.Name,
                        PhoneNumber = userToReturn.PhoneNumber
                    };

                    return "";

                }
                else
                {
                    return result.Errors.FirstOrDefault().Description;
                }

            }
            catch (Exception ex)
            {

            }
            return "Error Encountered";

        }
        public async Task<string> CreateRolePermissionRequest(RolePermission model)
        {
            try
            {
                var result = await _db.RolePermissions.AddAsync(model);
                _db.SaveChanges();
            }
            catch (Exception ex)
            {
                return "Failed to Add request";
            }
            return "";
        }
        public async Task<PermissionResponseDto> GetPendingPermissionList(string userEmail)
        {

            try
            {
                var appUser = await _userManager.FindByEmailAsync(userEmail);
                if (appUser == null)
                {
                    _permissionResponseDto.Result = null;
                    _permissionResponseDto.Error = "User not found Or not have Admin Access";
                    return _permissionResponseDto;
                }

                bool isAdmin = await _userManager.IsInRoleAsync(appUser, "Admin");
                if (!isAdmin)
                {
                    _permissionResponseDto.Result = null;
                    _permissionResponseDto.Error = "Access denied. Admin role required.";
                    return _permissionResponseDto;
                }

                var pendingList = await _db.RolePermissions
                .Where(x => x.Status == "Pending")
                 .Select(x => new RolePermission
                 {
                     Id = x.Id,
                     Role = x.Role,
                     IssuedBy = x.IssuedBy,
                     AskedByUserName = x.AskedByUserName,
                     Email = x.Email,
                     Status = x.Status,
                     RequestedDateTime = x.RequestedDateTime,
                     UpdatedDateTime = x.UpdatedDateTime
                 })
                .ToListAsync();

                _permissionResponseDto.Result = pendingList;
                
            }
            catch(Exception ex)
            {
                _permissionResponseDto.Result = false;
                _permissionResponseDto.Error = $"Failed to find Result: '{ex.Message}'";
                return _permissionResponseDto;
            }
            return _permissionResponseDto;
            }
        public async Task<PermissionResponseDto> UpsertPermission(RolePermissionUpdateDto rolePermissionUpdateDto,string adminUser)
        {
            try
            {
                var appUser = await _userManager.FindByEmailAsync(adminUser);
                if (appUser == null)
                {
                    _permissionResponseDto.Result = null;
                    _permissionResponseDto.Error = "User not found Or not have Admin Access";
                    return _permissionResponseDto;
                }

                bool isAdmin = await _userManager.IsInRoleAsync(appUser, "Admin");
                if (!isAdmin)
                {
                    _permissionResponseDto.Result = null;
                    _permissionResponseDto.Error = "Access denied. Admin role required.";
                    return _permissionResponseDto;
                }
                RolePermission pendingPerson = await _db.RolePermissions.FirstOrDefaultAsync(x => x.Id == rolePermissionUpdateDto.RequestId);
                if (pendingPerson != null)
                {
                    pendingPerson.UpdatedDateTime = rolePermissionUpdateDto.UpdatedDateTime;
                    pendingPerson.Status=rolePermissionUpdateDto.Status;
                    pendingPerson.IssuedBy = adminUser;
                }
                
                if (pendingPerson.Status == "Approved" && isAdmin)
                {
                    var IsSuccess = assignRole(pendingPerson.Email, pendingPerson.Role);
                    _permissionResponseDto.IsSuccess = IsSuccess.Result;
                }
                _db.RolePermissions.Update(pendingPerson);
                await _db.SaveChangesAsync();
                _permissionResponseDto.Result = pendingPerson;
                return _permissionResponseDto;
            }
            catch (Exception ex) {
                _permissionResponseDto.Error = ex.Message;
                _permissionResponseDto.IsSuccess= false;
                return _permissionResponseDto;
            }






        }
    }
}
 
