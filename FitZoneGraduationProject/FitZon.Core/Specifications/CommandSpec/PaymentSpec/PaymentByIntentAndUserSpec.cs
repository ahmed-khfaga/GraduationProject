using FitZone.Core.Entitys.PaymentEntity;

namespace FitZone.Core.Specifications.CommandSpec.PaymentSpec
{
    public class PaymentByIntentAndUserSpec : BaseSpecatifications<Payment>
    {
        public PaymentByIntentAndUserSpec(string userId, string paymentIntentId)
            : base(p => p.UserId == userId && p.PaymentIntentId == paymentIntentId)
        {
            Includes.Add(p => p.MembershipPlan);
            OrderByDescending = p => p.CreatedAt;
        }
    }
}
