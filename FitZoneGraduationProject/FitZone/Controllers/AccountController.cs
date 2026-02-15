using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FitZone.Core.Entitys.Identity;
using FitZone.Core.Enums;
using FitZone.Service.DTOs;
using FitZone.Service.Errors;
using FitZone.Services.Contract;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
namespace FitZone.APIs.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly IAuthService _authService;

        public AccountController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("Register")]
        public async Task<ActionResult<RegisterUserDTOs>> Register([FromForm] RegisterUserDTOs registerDto)
        {

            var result = await _authService.RegisterAsync(registerDto);

            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(new ApiException(400, result.Message));
        }


        [HttpPost("Login")]
        public async Task<ActionResult<LoginUserDTOs>> Login(LoginUserDTOs loginDto)
        {

            var result = await _authService.LoginAsync(loginDto);
            if (result.IsSuccess) 
            {
                return Ok(result);
            }

            return Unauthorized(new ApiException(401, result.Message));
        }
    }
}
