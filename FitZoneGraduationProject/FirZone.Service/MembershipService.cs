using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FitZone.Core.Entitys;
using FitZone.Core.Repository.Contract;
using FitZone.Core.Specifications;
using FitZone.Core.Specifications.CommandSpec;
using FitZone.Core.Specifications.CommandSpec.ProfileSpec;
using FitZone.Service.DTOs;
using FitZone.Service.Services.Contract;

namespace FitZone.Service
{
    public class MembershipService : IMembershipService
    {
        private readonly IGenericRepository<MembershipPlan> _membershipPlanRepo;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public MembershipService(IGenericRepository<MembershipPlan> membershipPlanRepo,IMapper mapper, IUnitOfWork unitOfWork)
        {
            _membershipPlanRepo = membershipPlanRepo;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<MembershipPlansDto>> GetAllMembershipsPlan()
        {
            var spic = new MembershipWithPlan();
            //var result = await _membershipPlanRepo.GetAllWithSpecAsync(spic);
            var result = await _unitOfWork.Repository<MembershipPlan>().GetAllWithSpecAsync(spic);

            return _mapper.Map<IEnumerable<MembershipPlansDto>>(result);
        }

        public async Task<IEnumerable<MembershipWithPricePlanDto>> GetMembershipsByDurationAsync(int duration)
        {
            

            var spic = new MembershipWithPlan(duration); // membership plan  30 day should get 2 row 1 Standard and 1 Premium 

           // var membershipPlanDB = await _membershipPlanRepo.GetAllWithSpecAsync(spic);
            var membershipPlanDB = await _unitOfWork.Repository<MembershipPlan>().GetAllWithSpecAsync(spic);
            if(membershipPlanDB is not null) 
            {
                return _mapper.Map<IEnumerable<MembershipWithPricePlanDto>>(membershipPlanDB);
            }

            return Enumerable.Empty<MembershipWithPricePlanDto>();
        }

        public async Task<bool> HasPremiumMembership(string applicationUserId)
        {
            // 1. get trainee
            var trainee = await _unitOfWork.Repository<Trainee>()
                .GetWithSpecAsync(new TraineeByUserIdSpec(applicationUserId));

            if (trainee == null)
                return false;
            // 2. get membership
            var membership = await _unitOfWork.Repository<TraineeMembership>()
                .GetWithSpecAsync(new ActiveMembershipSpec(trainee.Id));

            if (membership == null)
                return false;
            // 3. check premium
            return membership.MembershipPlan.Membership.IsPremium;
        }
    }
}
