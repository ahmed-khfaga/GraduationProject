using AutoMapper;
using FitZone.APIs.DTOs;
using FitZone.Core.Entitys;
using FitZone.Core.Repository.Contract;
using FitZone.Core.Specifications.CommandSpec;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FitZone.APIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MembershipController : ControllerBase
    {
        private readonly IGenericRepository<MembershipPlan> _membershipPlanRepo;
        private readonly IMapper _mapper;

        public MembershipController(IGenericRepository<MembershipPlan> membershipPlanRepo , IMapper mapper)
        {
            _membershipPlanRepo = membershipPlanRepo;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task <IActionResult> GetAllMembership()  // should get standard with month and premiumn with month with all descrptions
        {
            // need get membership from db where duration in days  = 30 

            int duration = 30; // month 

            var spic = new MembershipWithPlan(duration); // membership plan  30 day should get 2 row 1 Standard and 1 Premium 

            var membershipPlanDB = await _membershipPlanRepo.GetAllWithSpecAsync(spic); // get plan with fillter days 

            if (membershipPlanDB != null)
            {
                //var result = _mapper.Map<IEnumerable <MembershipPlan>, IEnumerable<MembershipWithPricePlanDTOs>>(membershipPlanDB);
                var result = _mapper.Map<IEnumerable<MembershipWithPricePlanDTOs>>(membershipPlanDB);

                return Ok(result);
            }

            return NotFound();        
        }
    }
}
