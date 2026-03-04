using FitZone.Core.Entitys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Core.Specifications.CommandSpec.ProfileSpec
{
    // Trainee by ApplicationUserId
    public class TraineeByUserIdSpec : BaseSpecatifications<Trainee>
    {
        public TraineeByUserIdSpec(string appUserId) : base(t => t.ApplicationUserId == appUserId)
        {
            Includes.Add(t => t.ApplicationUser);
        }
    }
}
