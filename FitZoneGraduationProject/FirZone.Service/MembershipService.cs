using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FitZone.Core.Entitys;
using FitZone.Core.Repository.Contract;
using FitZone.Core.Specifications.CommandSpec;
using FitZone.Service.DTOs;
using FitZone.Service.Services.Contract;

namespace FitZone.Service
{
    public class MembershipService : IMembershipService
    {
        private readonly IGenericRepository<MembershipPlan> _membershipPlanRepo;
        private readonly IMapper _mapper;


        public MembershipService(IGenericRepository<MembershipPlan> membershipPlanRepo,IMapper mapper)
        {
            _membershipPlanRepo = membershipPlanRepo;
            _mapper = mapper;
        }
        public async Task<IEnumerable<MembershipWithPricePlanDTOs>> GetMembershipsByDurationAsync(int duration)
        {
            

            var spic = new MembershipWithPlan(duration); // membership plan  30 day should get 2 row 1 Standard and 1 Premium 

            var membershipPlanDB = await _membershipPlanRepo.GetAllWithSpecAsync(spic);

            if(membershipPlanDB is not null) 
            {
                return _mapper.Map<IEnumerable<MembershipWithPricePlanDTOs>>(membershipPlanDB);
            }

            return Enumerable.Empty<MembershipWithPricePlanDTOs>();
        }
    }
}
