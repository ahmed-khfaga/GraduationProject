using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using FitZone.Core.Entitys.Identity;
using FitZone.Core.Entitys;
using FitZone.Core.Enums;
using FitZone.Core.Repository.Contract;
using FitZone.Service.DTOs;
using FitZone.Service.Errors;
using FitZone.Service.HelperAuth;
using FitZone.Services.Contract;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace FitZone.Service
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;

        public AuthService(
            IConfiguration configuration,
            UserManager<ApplicationUser> userManager,
            IUnitOfWork unitOfWork)
        {
            _configuration = configuration;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
        }
        public async Task<string> CreateTokenAsync(ApplicationUser user)
        {

             var userClaim = new List<Claim>() 
             {
                 new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email,user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())

             };

            var userRoles = await _userManager.GetRolesAsync(user);

            foreach (var role in userRoles)
            {
                userClaim.Add(new Claim(ClaimTypes.Role, role));
            }

            var signInKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:SecretKey"]));

            var _signingCredentials = new SigningCredentials(signInKey, SecurityAlgorithms.HmacSha256Signature);

            var myToken = new JwtSecurityToken(
                            issuer: _configuration["JWT:ValidIssuer"],
                            audience: _configuration["JWT:ValidAudience"],
                            expires: DateTime.UtcNow.AddDays(double.Parse(_configuration["JWT:expiresDateinDay"])),
                            claims: userClaim,
                            signingCredentials: _signingCredentials
                            );

            return new JwtSecurityTokenHandler().WriteToken(myToken);
        }

        public async Task<AuthResultDto> LoginAsync(LoginUserDto model)
        {
            ApplicationUser user = await _userManager.FindByEmailAsync(model.Email);
            if (user is null) 
            {
                return new AuthResultDto
                {
                    IsSuccess = false,
                    Message = "Email or Password are wrong"
                };
            }
            bool isValidPassword = await _userManager.CheckPasswordAsync(user, model.Password);

            if (!isValidPassword)
            {
                return new AuthResultDto
                {
                    IsSuccess = false,
                    Message = "Email or Password are wrong"
                };
            }

            var token = await CreateTokenAsync(user);
            

            return new AuthResultDto
            {
                IsSuccess = true,
                Message = "Login successful",
                Token = token
            };

        }

        public async Task<AuthResultDto> RegisterAsync(RegisterUserDto model)
        {
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)

                return new AuthResultDto
                {
                    IsSuccess = false,
                    Message = "Email already exists"
                };


            // first work on photo if user enter photo 
            string photoPath = "images/default.jpg";
            if (model.Photo is not null)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.Photo.FileName);
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/Trainees");

                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                var filePath = Path.Combine(folderPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.Photo.CopyToAsync(stream);
                }
                photoPath = $"images/Trainees/{fileName}";
            }
                // Save DB
             ApplicationUser appUser = new ApplicationUser();

             appUser.F_Name = model.FirstName;
             appUser.L_Name = model.LastName;
             appUser.UserName = $"{model.FirstName}{model.LastName}";
             appUser.Email = model.Email;
             appUser.PhotoUrl = photoPath;
             appUser.Role = UserRole.Trainee;

             IdentityResult result = await _userManager.CreateAsync(appUser, model.Password);
            if (!result.Succeeded)
            {
                return new AuthResultDto
                {
                    IsSuccess = false,
                    Message = string.Join(", ", result.Errors.Select(e => e.Description))
                };
            }

            var roleResult = await _userManager.AddToRoleAsync(appUser, "Trainee");
            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(appUser);
                return new AuthResultDto
                {
                    IsSuccess = false,
                    Message = string.Join(", ", roleResult.Errors.Select(e => e.Description))
                };
            }

            _unitOfWork.Repository<Trainee>().Add(new Trainee
            {
                ApplicationUserId = appUser.Id,
                Gender = "NotSpecified",
                PhotoUrl = photoPath
            });

            await _unitOfWork.CompleteAsync();

            return new AuthResultDto
            {
                IsSuccess = true,
                Message = "Account created successfully"
            };
        }
    }
}
