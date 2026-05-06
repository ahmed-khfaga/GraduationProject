using FitZone.Core.Entitys;

namespace FitZone.Core.Specifications.CommandSpec.MembershipSpec
{
    public class MembershipPlanByIdSpec : BaseSpecatifications<MembershipPlan>
    {
        public MembershipPlanByIdSpec(int membershipPlanId) : base(m => m.Id == membershipPlanId)
        {
            Includes.Add(m => m.Membership);
        }
    }
}
