using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FitZone.APIs.DTOs;
using FitZone.Core.Entitys.Identity;
using FitZone.Core.Enums;
using FitZone.Core.Services.Contract;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace FitZone.APIs.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IAuthService _authService;

        public AccountController(UserManager<ApplicationUser> _userManager, IAuthService authService)
        {
            userManager = _userManager;
            _authService = authService;
        }

        [HttpPost("Register")]
        public async Task<ActionResult<RegisterUserDTOs>> Register([FromForm] RegisterUserDTOs registerDto)
        {

            // first check if email is already exist or not 
            var existingUser = await userManager.FindByEmailAsync(registerDto.Email);
            if (existingUser != null)
                return BadRequest("Email already exists");

            if (ModelState.IsValid)
            {
                // first work on photo if user enter photo 
                string photoPath = "images/default.jpg";
                if (registerDto.Photo is not null)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(registerDto.Photo.FileName);


                    var folderPath = Path.Combine(Directory.GetCurrentDirectory(),"wwwroot/images/Trainees");
                   
                    if (!Directory.Exists(folderPath))
                        Directory.CreateDirectory(folderPath);

                    var filePath = Path.Combine(folderPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await registerDto.Photo.CopyToAsync(stream);
                    }

                    photoPath = $"images/Trainees/{fileName}";
                }

                // Save DB
                ApplicationUser appUser = new ApplicationUser();

                appUser.F_Name = registerDto.FirstName;
                appUser.L_Name = registerDto.LastName;                             
                appUser.UserName = $"{registerDto.FirstName}{registerDto.LastName}";
                appUser.Email = registerDto.Email;
                appUser.PhotoUrl = photoPath;
                appUser.Role = UserRole.Trainee;

                IdentityResult result = await userManager.CreateAsync(appUser, registerDto.Password);
                if (result.Succeeded)
                {
                    return Ok("created successfully");
                }
                foreach (var item in result.Errors)
                {
                    ModelState.AddModelError("Password", item.Description);
                }

            }

            return BadRequest(ModelState);
        }


        [HttpPost("Login")]
        public async Task<ActionResult<LoginUserDTOs>> Login(LoginUserDTOs log)
        {

            if (ModelState.IsValid)
            {
                // check 
                ApplicationUser user = await userManager.FindByEmailAsync(log.Email);

                if (user is not null)
                {
                    bool found = await userManager.CheckPasswordAsync(user, log.Password);
                    if (found)
                    {
                        var token = await _authService.CreateTokenAsync(user, userManager);
                        return Ok(token);                       
                    }
                }
                ModelState.AddModelError("Email", "Email or Password are wrong!");
            }
            return BadRequest(ModelState);
        }
    }
}
