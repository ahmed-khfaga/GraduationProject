using FitZone.Core.Entitys;
using FitZone.Core.Entitys.PaymentEntity;
using FitZone.Core.Enums;
using FitZone.Core.Repository.Contract;
using FitZone.Core.Specifications.CommandSpec.MembershipSpec;
using FitZone.Core.Specifications.CommandSpec.PaymentSpec;
using FitZone.Core.Specifications.CommandSpec.ProfileSpec;
using FitZone.Core.Specifications;
using FitZone.Service.DTOs.PaymentDTOs;
using FitZone.Service.Helpers;
using FitZone.Service.Services.Contract.Payment;

namespace FitZone.Service
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PaymentService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PaymentIntentDto> CreateMembershipPaymentIntentAsync(string userId, int membershipPlanId)
        {
            var plan = await _unitOfWork.Repository<MembershipPlan>()
                .GetWithSpecAsync(new MembershipPlanByIdSpec(membershipPlanId))
                ?? throw new InvalidOperationException("Membership plan not found.");

            var paymentIntentId = Guid.NewGuid().ToString("N");
            var payment = new Payment
            {
                UserId = userId,
                MembershipPlanId = membershipPlanId,
                Amount = plan.Price,
                Status = PaymentStatus.Pending,
                PaymentIntentId = paymentIntentId,
                CreatedAt = DateTime.UtcNow
            };

            _unitOfWork.Repository<Payment>().Add(payment);
            await _unitOfWork.CompleteAsync();

            return new PaymentIntentDto
            {
                PaymentIntentId = paymentIntentId,
                MembershipPlanId = membershipPlanId,
                Amount = plan.Price,
                Status = PaymentStatus.Pending.ToString()
            };
        }

        public async Task<PaymentStatusDto> ConfirmMembershipPaymentAsync(string userId, string paymentIntentId, string cardNumber)
        {
            if (!PaymentCardValidator.TryNormalize(cardNumber, out var normalizedCard, out var cardError))
            {
                throw new InvalidOperationException(cardError ?? "Invalid card number.");
            }

            var payment = await _unitOfWork.Repository<Payment>()
                .GetWithSpecAsync(new PaymentByIntentAndUserSpec(userId, paymentIntentId))
                ?? throw new InvalidOperationException("Payment intent not found.");

            if (payment.Status == PaymentStatus.Paid)
            {
                return new PaymentStatusDto
                {
                    PaymentIntentId = paymentIntentId,
                    Status = PaymentStatus.Paid.ToString(),
                    CardLastFour = payment.CardLastFour
                };
            }

            if (payment.Status == PaymentStatus.Failed)
                throw new InvalidOperationException("Payment already failed.");

            if (payment.MembershipPlan is null)
                throw new InvalidOperationException("Membership plan not found for this payment.");

            if (payment.Amount != payment.MembershipPlan.Price)
            {
                payment.Status = PaymentStatus.Failed;
                _unitOfWork.Repository<Payment>().Update(payment);
                await _unitOfWork.CompleteAsync();
                throw new InvalidOperationException("Payment amount mismatch.");
            }

            payment.Status = PaymentStatus.Paid;
            payment.CardLastFour = normalizedCard[^4..];
            _unitOfWork.Repository<Payment>().Update(payment);
            await _unitOfWork.CompleteAsync();

            // Business requirement: activate membership 5 seconds after successful payment confirmation.
            await Task.Delay(TimeSpan.FromSeconds(5));
            await ActivateMembershipFromPaymentAsync(userId, payment);

            return new PaymentStatusDto
            {
                PaymentIntentId = paymentIntentId,
                Status = PaymentStatus.Paid.ToString(),
                CardLastFour = payment.CardLastFour
            };
        }

        public async Task<bool> HasSuccessfulPaymentForPlanAsync(string userId, int membershipPlanId, string paymentIntentId)
        {
            if (string.IsNullOrWhiteSpace(paymentIntentId))
                return false;

            var payment = await _unitOfWork.Repository<Payment>()
                .GetWithSpecAsync(new PaymentByIntentAndUserSpec(userId, paymentIntentId));

            return payment is not null
                && payment.MembershipPlanId == membershipPlanId
                && payment.Status == PaymentStatus.Paid;
        }

        private async Task ActivateMembershipFromPaymentAsync(string userId, Payment payment)
        {
            var trainee = await _unitOfWork.Repository<Trainee>()
                .GetWithSpecAsync(new TraineeByUserIdSpec(userId))
                ?? throw new InvalidOperationException("Trainee profile not found.");

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
                MembershipPlanId = payment.MembershipPlanId,
                IsActive = true,
                StartDate = now,
                EndDate = now.AddDays(payment.MembershipPlan.DurationInDays)
            };

            _unitOfWork.Repository<TraineeMembership>().Add(newMembership);
            await _unitOfWork.CompleteAsync();
        }
    }
}
