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
        private readonly IGenericRepository<Membership> _membershipRepo;
        private readonly IGenericRepository<MembershipPlan> _membershipPlanRepo;

        public MembershipController(IGenericRepository<Membership> membershipRepo, IGenericRepository<MembershipPlan> membershipPlanRepo)
        {
            _membershipRepo = membershipRepo;
            _membershipPlanRepo = membershipPlanRepo;
        }



        [HttpGet]
        public async Task <IActionResult> GetAllMembership()  // should get standard with month and premiumn with month with all descrptions
        {
            //var membershipToClient = new MembershipWithPricePlanDTOs();
            // need get membership from db where duration in days  = 30 

            int duration = 30; // month 

            var spic = new MembershipWithPlan(duration); // membership plan  30 day should get 2 row 1 standard and 1 prem 

            var membershipPlanDB = await _membershipPlanRepo.GetAllWithSpecAsync(spic); // get plan with fillter days 



            if (membershipPlanDB != null)
            {


                var membershipToClient = membershipPlanDB.Select(mp => new MembershipWithPricePlanDTOs
                {
                    Id = mp.ID,
                    Name = mp.Membership.Name,
                    Description = mp.Membership.Description,
                    Price = mp.Price,
                    Title = mp.Title
                }).ToList();

                return Ok(membershipToClient);

            }
            return NotFound();        
        }
    }
}
