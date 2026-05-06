using AutoMapper;
using FitZone.Core.Entitys;
using FitZone.Core.Repository.Contract;
using FitZone.Core.Specifications.CommandSpec;
using FitZone.Service.DTOs;
using FitZone.Service.Errors;
using FitZone.Service.Services.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FitZone.APIs.Controllers
{
    public class MembershipController : BaseApiController
    {
        private readonly IMembershipService _membershipService;

        public MembershipController(IMembershipService membershipService)
        {
            _membershipService = membershipService;
        }
        [HttpGet]
        public async Task <ActionResult<MembershipWithPricePlanDto>> GetAllMembershipInMonth()  // should get standard with month and premiumn with month with all descrptions
        {

            int duration = 30; // month 

            var result = await _membershipService.GetMembershipsByDurationAsync(duration);

            if(result.Any())
            {
                return Ok(result);
            }

            return NotFound(new ApiException(404, "No Memberships found"));        
        }

        [HttpGet("Plans")]
        public async Task<ActionResult<MembershipWithPricePlanDto>> GetMembershipPlan() 
        {
            var result = await _membershipService.GetAllMembershipsPlan();

            return Ok(result);
        }

        [Authorize(Roles = "Trainee")]
        [HttpPost("activate")]
        public async Task<ActionResult<MembershipStatusDto>> ActivateMembership([FromBody] MembershipActivationDto dto)
        {
            var appUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(appUserId))
                return Unauthorized(new ApiException(401, "Invalid user token."));

            var result = await _membershipService.ActivateMembershipAsync(appUserId, dto.MembershipPlanId);
            return Ok(result);
        }

        [Authorize(Roles = "Trainee")]
        [HttpGet("me")]
        public async Task<ActionResult<MembershipStatusDto>> GetMyMembershipStatus()
        {
            var appUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(appUserId))
                return Unauthorized(new ApiException(401, "Invalid user token."));

            var result = await _membershipService.GetMyMembershipStatusAsync(appUserId);
            return Ok(result);
        }
    }
}
