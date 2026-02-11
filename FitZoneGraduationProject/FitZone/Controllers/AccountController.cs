using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FitZone.APIs.DTOs;
using FitZone.Core.Entitys.Identity;
using FitZone.Core.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace FitZone.APIs.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly UserManager<ApplicationUser> userManager;


        public AccountController(UserManager<ApplicationUser> _userManager)
        {
            userManager = _userManager;
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
                
                string fullName = registerDto.FirstName + registerDto.LastName;

                appUser.UserName = fullName;
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

                        var userClaim = new List<Claim>();

                        userClaim.Add(new Claim(ClaimTypes.NameIdentifier, user.Id));
                        userClaim.Add(new Claim(ClaimTypes.Name, user.UserName));

                        userClaim.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));


                        var userRoles = await userManager.GetRolesAsync(user);

                        foreach (var roleName in userRoles)
                        {
                            userClaim.Add(new Claim(ClaimTypes.Role, roleName));
                        }


                        var signInKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("YYYUUUKKK@122381##4dsfbnlll120947"));


                        var _signingCredentials = new SigningCredentials(signInKey, SecurityAlgorithms.HmacSha256);

                        //design token 
                        var myToken = new JwtSecurityToken(
                            issuer: "http://localhost:5234/",
                            audience: "http://localhost:3000/",
                            expires: DateTime.Now.AddHours(1),
                            claims: userClaim,
                            signingCredentials: _signingCredentials

                            );

                        //generate token response

                        return Ok(new
                        {
                            token = new JwtSecurityTokenHandler().WriteToken(myToken),
                            expiration = DateTime.Now.AddHours(1)  // myToken.ValidTo
                        }

                            );

                    }
                }
                ModelState.AddModelError("Email", "Email or Password are wrong!");
                //generate token
            }
            return BadRequest(ModelState);
        }
    }
}
