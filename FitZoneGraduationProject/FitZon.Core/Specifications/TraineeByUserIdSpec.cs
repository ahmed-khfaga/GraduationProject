using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FitZone.Core.Entitys;

namespace FitZone.Core.Specifications
{
    internal class TraineeByUserIdSpec : BaseSpecatifications<Trainee>
    {
        public TraineeByUserIdSpec(string userId)
        : base(t => t.ApplicationUserId == userId)
        {
        }
    }
}
