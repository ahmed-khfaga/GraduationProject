using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FitZone.Core.Entitys.Identity;
using FitZone.Service.DTOs;
using FitZone.Service.HelperAuth;
using Microsoft.AspNetCore.Identity;

namespace FitZone.Services.Contract
{
    public interface IAuthService
    {
        Task<string> CreateTokenAsync(ApplicationUser user);
        Task<AuthResultDto> RegisterAsync(RegisterUserDto model);
        Task<AuthResultDto> LoginAsync(LoginUserDto model);
    }
}
