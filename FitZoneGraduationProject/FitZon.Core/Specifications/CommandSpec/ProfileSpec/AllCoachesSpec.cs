using FitZone.Core.Entitys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Core.Specifications.CommandSpec.ProfileSpec
{
    // All coaches list (public browse)
    public class AllCoachesSpec : BaseSpecatifications<Coach>
    {
        public AllCoachesSpec() : base()
        {
            Includes.Add(c => c.ApplicationUser);
            OrderByDescending = c => c.Rating!;
        }
    }
}
