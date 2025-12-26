using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Core.Entitys
{
    public class Membership
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }

        public bool IsPremium { get; set; }

        public virtual ICollection<TraineeMembership> TraineeMemberships { get; set; } = new HashSet<TraineeMembership>();

        public virtual ICollection<MembershipPlan> MembershipPlans { get; set; } = new HashSet<MembershipPlan>();


    }
}
