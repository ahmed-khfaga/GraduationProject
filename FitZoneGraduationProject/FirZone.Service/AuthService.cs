using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using FitZone.Core.Entitys.Identity;
using FitZone.Core.Services.Contract;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace FitZone.Service
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;

        public AuthService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<string> CreateTokenAsync(ApplicationUser user, UserManager<ApplicationUser> userManager)
        {

             var userClaim = new List<Claim>() 
             {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email,user.Email)
             };

            var userRoles = await userManager.GetRolesAsync(user);

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
    }
}
