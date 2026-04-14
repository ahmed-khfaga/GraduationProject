using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FitZone.Core.Entitys;

namespace FitZone.Core.Specifications
{
    public class ActivePremiumMembershipSpec : BaseSpecatifications<TraineeMembership>
    {
        public ActivePremiumMembershipSpec(string userId)
        : base(m =>
            m.TraineeID.ToString() == userId &&
            m.IsActive &&
            m.EndDate > DateTime.UtcNow)
        {
            Includes.Add(m => m.MembershipPlan);
            Includes.Add(m => m.MembershipPlan.Membership);
        }
    }
}
