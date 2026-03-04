using FitZone.Core.Entitys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Core.Specifications.CommandSpec.ProfileSpec
{
    // Coach public profile
    public class CoachByIdSpec : BaseSpecatifications<Coach>
    {
        public CoachByIdSpec(int coachId) : base(c => c.ID == coachId)
        {
            Includes.Add(c => c.ApplicationUser);
        }
    }
}
