using FitZone.Core.Entitys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Core.Specifications.CommandSpec.ProfileSpec
{
    // Coach by ApplicationUserId (JWT auth)
    public class CoachByUserIdSpec : BaseSpecatifications<Coach>
    {
        public CoachByUserIdSpec(string appUserId) : base(c => c.ApplicationUserId == appUserId)
        {
            Includes.Add(c => c.ApplicationUser);
        }
    }
}
