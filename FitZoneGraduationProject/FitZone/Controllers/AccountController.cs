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
using Microsoft.AspNetCore.Authorization;

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
        public async Task<ActionResult<RegisterUserDto>> Register([FromForm] RegisterUserDto registerDto)
        {

            var result = await _authService.RegisterAsync(registerDto);

            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(new ApiException(400, result.Message));
        }


        [HttpPost("Login")]
        public async Task<ActionResult<LoginUserDto>> Login(LoginUserDto loginDto)
        {

            var result = await _authService.LoginAsync(loginDto);
            if (result.IsSuccess) 
            {
                return Ok(result);
            }

            return Unauthorized(new ApiException(401, result.Message));
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<AuthUserDto>> Me()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized(new ApiException(401, "Invalid user token."));

            var result = await _authService.GetCurrentUserAsync(userId);

            if (result is null)
                return NotFound(new ApiException(404, "User not found."));

            return Ok(result);
        }
    }
}
