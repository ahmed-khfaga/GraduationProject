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
using FitZone.Core.Specifications.CommandSpec.MembershipSpec;
using FitZone.Core.Specifications.CommandSpec.ProfileSpec;
using FitZone.Service.DTOs;
using FitZone.Service.Services.Contract;
using FitZone.Service.Services.Contract.Payment;

namespace FitZone.Service
{
    public class MembershipService : IMembershipService
    {
        private readonly IGenericRepository<MembershipPlan> _membershipPlanRepo;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPaymentService _paymentService;

        public MembershipService(IGenericRepository<MembershipPlan> membershipPlanRepo, IMapper mapper, IUnitOfWork unitOfWork, IPaymentService paymentService)
        {
            _membershipPlanRepo = membershipPlanRepo;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _paymentService = paymentService;
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

        public async Task<MembershipStatusDto> ActivateMembershipAsync(string applicationUserId, int membershipPlanId, string paymentIntentId)
        {
            if (string.IsNullOrWhiteSpace(paymentIntentId))
                throw new InvalidOperationException("Payment intent is required.");

            var trainee = await _unitOfWork.Repository<Trainee>()
                .GetWithSpecAsync(new TraineeByUserIdSpec(applicationUserId))
                ?? throw new InvalidOperationException("Trainee profile not found.");

            var selectedPlan = await _unitOfWork.Repository<MembershipPlan>()
                .GetWithSpecAsync(new MembershipPlanByIdSpec(membershipPlanId))
                ?? throw new InvalidOperationException("Membership plan not found.");

            var hasValidPayment = await _paymentService
                .HasSuccessfulPaymentForPlanAsync(applicationUserId, selectedPlan.Id, paymentIntentId);
            if (!hasValidPayment)
                throw new InvalidOperationException("Membership activation requires a successful payment.");

            var currentActive = await _unitOfWork.Repository<TraineeMembership>()
                .GetWithSpecAsync(new ActiveMembershipSpec(trainee.Id));

            if (currentActive is not null)
            {
                currentActive.IsActive = false;
                currentActive.EndDate = DateTime.UtcNow;
                _unitOfWork.Repository<TraineeMembership>().Update(currentActive);
            }

            var now = DateTime.UtcNow;
            var newMembership = new TraineeMembership
            {
                TraineeId = trainee.Id,
                MembershipPlanId = selectedPlan.Id,
                IsActive = true,
                StartDate = now,
                EndDate = now.AddDays(selectedPlan.DurationInDays)
            };
            _unitOfWork.Repository<TraineeMembership>().Add(newMembership);

            await _unitOfWork.CompleteAsync();

            return new MembershipStatusDto
            {
                IsActive = true,
                MembershipPlanId = selectedPlan.Id,
                PlanTitle = selectedPlan.Title,
                MembershipName = selectedPlan.Membership.Name,
                IsPremium = selectedPlan.Membership.IsPremium,
                StartDate = newMembership.StartDate,
                EndDate = newMembership.EndDate
            };
        }

        public async Task<MembershipStatusDto> GetMyMembershipStatusAsync(string applicationUserId)
        {
            var trainee = await _unitOfWork.Repository<Trainee>()
                .GetWithSpecAsync(new TraineeByUserIdSpec(applicationUserId));

            if (trainee is null)
            {
                return new MembershipStatusDto { IsActive = false };
            }

            var membership = await _unitOfWork.Repository<TraineeMembership>()
                .GetWithSpecAsync(new ActiveMembershipSpec(trainee.Id));

            if (membership is null)
            {
                return new MembershipStatusDto { IsActive = false };
            }

            return new MembershipStatusDto
            {
                IsActive = true,
                MembershipPlanId = membership.MembershipPlanId,
                PlanTitle = membership.MembershipPlan.Title,
                MembershipName = membership.MembershipPlan.Membership.Name,
                IsPremium = membership.MembershipPlan.Membership.IsPremium,
                StartDate = membership.StartDate,
                EndDate = membership.EndDate
            };
        }
    }
}
