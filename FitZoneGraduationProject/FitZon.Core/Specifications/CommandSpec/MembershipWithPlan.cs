using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FitZone.Core.Entitys;

namespace FitZone.Core.Specifications.CommandSpec
{
    public class MembershipWithPlan : BaseSpecatifications<MembershipPlan>
    {
      
        public MembershipWithPlan() : base()
        {
            Includes.Add(m => m.Membership);
        }

        public MembershipWithPlan(int duration) : base( m => m.DurationInDays == duration ) // fillter with duration 
        {
            Includes.Add(m => m.Membership);
        }
    }
}
