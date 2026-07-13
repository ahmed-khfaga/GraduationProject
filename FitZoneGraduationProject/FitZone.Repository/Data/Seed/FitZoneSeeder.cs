using FitZone.Core.Entitys.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FitZone.Repository.Data.Seed
{
    public static class FitZoneSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<FitContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            await context.Database.MigrateAsync();

            await RoleSeeder.SeedAsync(roleManager);
            await MembershipSeeder.SeedAsync(context);
            await UserSeeder.SeedAsync(context, userManager);
            await TrackSeeder.SeedAsync(context);
            await ExerciseSeeder.SeedAsync(context);
            await ProgramSeeder.SeedAsync(context);

            // ── NEW: nutrition system ─────────────────────────────────────
            // Must run AFTER UserSeeder (needs Coach Ahmed + Trainee Mohamed)
            // and AFTER ProgramSeeder (optionally links to Push Pull Legs program).
            await NutritionSeeder.SeedAsync(context);
        }
    }
}