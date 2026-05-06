using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FitZone.Core.Comman;

namespace FitZone.Core.Entitys
{
    public class Membership : BaseEntity
    {

        public string Name { get; set; }
        public string Description { get; set; }

        public bool IsPremium { get; set; }

        public virtual ICollection<MembershipPlan> MembershipPlans { get; set; } = new HashSet<MembershipPlan>();


    }
}
