using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FitZone.Core.Comman;
using FitZone.Core.Entitys.Identity;
using FitZone.Core.Enums;

namespace FitZone.Core.Entitys.PaymentEntity
{
    public class Payment : BaseEntity
    {
        public string UserId { get; set; }
        public int MembershipPlanId { get; set; }

        public decimal Amount { get; set; }

        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        public string? PaymentIntentId { get; set; }

        public string? CardLastFour { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        public virtual ApplicationUser User { get; set; }
        public virtual MembershipPlan MembershipPlan { get; set; }
    }
}
