using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FitZone.Core.Comman;

namespace FitZone.Core.Entitys
{
    public class MembershipPlan : BaseEntity
    {

        [ForeignKey("MembershipID")]
        public int MembershipID { get; set; }

        public int DurationInDays { get; set; }

        public decimal Price { get; set; }


        public string Title { get; set; } // "1 Month", "3 Months", "1 Year"

        public virtual Membership Membership { get; set; }

    }
}
