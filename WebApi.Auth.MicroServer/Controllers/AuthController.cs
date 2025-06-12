using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WebApi.Auth.MicroServer.Model;
using WebApi.Auth.MicroServer.Model.Dto;
using WebApi.Auth.MicroServer.Service.IService;

namespace WebApi.Auth.MicroServer.Controllers
{
    [Route("api/v1.0/Auth")]
    [ApiController]
    public class AuthController:ControllerBase
    {
        private readonly IAuthService _authService;
        protected ResponseDto _responseDto;
        private AdminSettings _adminSettings;
        public AuthController(IAuthService authService, ResponseDto responseDto, IOptions<AdminSettings> adminSettings)
        {
            _authService = authService;
            _responseDto = new();
            _adminSettings = adminSettings.Value;
        }

        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody] RegistrationRequestDto model)
        {
            var error = await _authService.Regestiration(model);
            if (!String.IsNullOrEmpty(error))
            {
                _responseDto.IsSuccess = false;
                _responseDto.Message = error;
                return BadRequest(_responseDto);
            }
            return Ok(_responseDto);
              
        }
        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto model)
        {
            var loginResponse= await _authService.Login(model);
            if(loginResponse.User==null)
            {
                _responseDto.IsSuccess = false;
                _responseDto.Message = "UserName or Password is incorrect";
                return BadRequest(_responseDto);    
            }
            _responseDto.Result = loginResponse;
            return Ok(_responseDto);
        }
        [HttpPost]
        [Route("AssignRole")]
        public async Task<IActionResult> AssignRole([FromBody] RegistrationRequestDto model)
        {
            var email =model.Email.ToLower();
            var role = model.Role;
            var secretCode = model.SecretKey;
            if (role != "User" && secretCode != null && secretCode==_adminSettings.SecretCode)
            {
                var assignRoleSuccessful = await _authService.assignRole(model.Email, model.Role);
                if (!assignRoleSuccessful)
                {
                    _responseDto.IsSuccess = false;
                    _responseDto.Message = "Error encountered & Wrong Key";
                    return BadRequest(_responseDto);
                }
                return Ok(_responseDto);


            }
            if (role != "User" )
            {
                var rolePermission = new RolePermission()
                {
                    Role = model.Role,
                    AskedByUserName = model.Name,
                    Email = model.Email,
                    RequestedDateTime = DateTime.Now,
                };
                // Save the permission request (you need this method implemented in your service)
                var requestSaved = await _authService.CreateRolePermissionRequest(rolePermission);
                if (!String.IsNullOrEmpty(requestSaved))
                {
                    _responseDto.IsSuccess = false;
                    _responseDto.Message = "Failed to create permission request.";
                    return BadRequest(_responseDto);
                }

                _responseDto.Message = $"Assigned 'User' role temporarily. '{model.Role}' role request is pending approval.";
                return Ok(_responseDto);
            }
            else
            {
                var assignRoleSuccessful = await _authService.assignRole(model.Email, "User");
                if (!assignRoleSuccessful)
                {
                    _responseDto.IsSuccess = false;
                    _responseDto.Message = "Error encountered";
                    return BadRequest(_responseDto);
                }
                return Ok(_responseDto);
            }
        }
        [HttpGet]
        [Route("pendingpermission")]
        public async Task<IActionResult> GetPendingPermissionRequest([FromQuery] string userEmail)
        {
            var permissionListResponse = await _authService.GetPendingPermissionList(userEmail);
            if (permissionListResponse == null)
            {
                _responseDto.IsSuccess=false;
                _responseDto.Message = "Failed to Reterive Resule";
                return BadRequest(_responseDto);
            }
            _responseDto.Result = permissionListResponse;
            return Ok(_responseDto);
        }
        [HttpPost]
        [Route("authorisingpermission")]
        public async Task<IActionResult> UpsertPermissionRequest([FromBody] RolePermissionUpdateDto rolePermissionUpdateDto, [FromQuery] string adminUser)
        {
            var permissionResponse = await _authService.UpsertPermission(rolePermissionUpdateDto, adminUser);
            if (!permissionResponse.IsSuccess)
            {
                _responseDto.IsSuccess=false;
                _responseDto.Message = "Failed to Upsert Role Permisson";
                return BadRequest(_responseDto);
            }
            _responseDto.Result=permissionResponse;
            return Ok(_responseDto);
        }


    }
}
