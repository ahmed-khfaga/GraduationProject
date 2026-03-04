using FitZone.Core.Entitys;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Repository.Data.Seed
{
    public static class MembershipSeeder
    {
        public static async Task SeedAsync(FitContext context)
        {
            if (await context.Memberships.AnyAsync()) return;

            var standard = new Membership
            {
                Name = "Standard",
                Description = "Full access to all published workout programs across all tracks, " +
                              "the complete exercise library, and the nutrition plan catalogue.",
                IsPremium = false,
                MembershipPlans = new List<MembershipPlan>
                {
                    new MembershipPlan { Title = "1 Month",  DurationInDays = 30,  Price = 29.99m  },
                    new MembershipPlan { Title = "3 Months", DurationInDays = 90,  Price = 79.99m  },
                    new MembershipPlan { Title = "1 Year",   DurationInDays = 365, Price = 299.99m }
                }
            };

            var premium = new Membership
            {
                Name = "Premium",
                Description = "Everything in Standard plus a dedicated coach, direct messaging, " +
                              "and a custom program built specifically for you.",
                IsPremium = true,
                MembershipPlans = new List<MembershipPlan>
                {
                    new MembershipPlan { Title = "1 Month",  DurationInDays = 30,  Price = 99.99m   },
                    new MembershipPlan { Title = "3 Months", DurationInDays = 90,  Price = 279.99m  },
                    new MembershipPlan { Title = "1 Year",   DurationInDays = 365, Price = 1099.99m }
                }
            };

            context.Memberships.AddRange(standard, premium);
            await context.SaveChangesAsync();
        }
    }
}
