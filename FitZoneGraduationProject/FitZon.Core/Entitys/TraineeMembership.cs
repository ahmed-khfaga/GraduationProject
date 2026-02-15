using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FitZone.Core.Comman;

namespace FitZone.Core.Entitys
{
    public class TraineeMembership : BaseEntity
    {


        [ForeignKey("Trainee")]
        public int TraineeID { get; set; }

        [ForeignKey("MembershipPlan")]
        public int MembershipPlanID { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime StartDate { get; set; } = DateTime.Now;

        public DateTime? EndDate { get; set; }

        public virtual Trainee Trainee { get; set; }
        public virtual MembershipPlan MembershipPlan { get; set; }

    }
}
