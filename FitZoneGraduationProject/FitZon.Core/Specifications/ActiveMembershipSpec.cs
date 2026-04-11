using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FitZone.Core.Entitys;

namespace FitZone.Core.Specifications
{
    public class ActiveMembershipSpec : BaseSpecatifications<TraineeMembership>
    {
        public ActiveMembershipSpec(int traineeId)
        : base(m =>
            m.TraineeID == traineeId &&
            m.IsActive &&
            m.EndDate > DateTime.UtcNow)
        {
            Includes.Add(m => m.MembershipPlan);
            Includes.Add(m => m.MembershipPlan.Membership);
        }
    }
}
