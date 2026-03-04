using FitZone.Core.Entitys;
using FitZone.Core.Entitys.Identity;
using FitZone.Core.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Repository.Data.Seed
{
    public static class UserSeeder
    {
        public static async Task SeedAsync(FitContext context, UserManager<ApplicationUser> userManager)
        {
            // ── Admin ────────────────────────────────────────────────
            // Each user gets its own guard so a partial run can be safely resumed.
    
           
            await CreateUserAsync(
                userManager,
                firstName: "Sara",
                lastName: "Hassan",
                email: "admin@fitzone.com",
                password: "Admin@1234",
                role: UserRole.Admin,
                identityRole: "Admin",
                photo: "images/default.jpg"
            );

            // ── Coach 1 — Ahmed (Strength specialist) ────────────────
            var coachUser1 = await CreateUserAsync(
                userManager,
                firstName: "Ahmed",
                lastName: "Kareem",
                email: "ahmed.coach@fitzone.com",
                password: "Coach@1234",
                role: UserRole.Coach,
                identityRole: "Coach",
                photo: "images/Coaches/ahmed.jpg"
            );

            if (coachUser1 is not null && !await context.Coachs.AnyAsync(c => c.ApplicationUserId == coachUser1.Id))
            {
                context.Coachs.Add(new Coach
                {
                    ApplicationUserId = coachUser1.Id,
                    About = "Certified strength & conditioning specialist with a background in " +
                                        "powerlifting. I design evidence-based programs that deliver real, " +
                                        "measurable results for beginners and advanced athletes alike.",
                    YearsOfExperience = 7,
                    Rating = 4.8m,
                    Price = 60.00m,
                    HireDate = new DateTime(2024, 1, 15)
                });
                await context.SaveChangesAsync();
            }

            // ── Coach 2 — Nour (Conditioning & fat loss) ────────────
            var coachUser2 = await CreateUserAsync(
                userManager,
                firstName: "Nour",
                lastName: "Eldin",
                email: "nour.coach@fitzone.com",
                password: "Coach@1234",
                role: UserRole.Coach,
                identityRole: "Coach",
                photo: "images/Coaches/nour.jpg"
            );

            if (coachUser2 is not null && !await context.Coachs.AnyAsync(c => c.ApplicationUserId == coachUser2.Id))
            {
                context.Coachs.Add(new Coach
                {
                    ApplicationUserId = coachUser2.Id,
                    About = "Sports science graduate specialising in conditioning, HIIT, and " +
                                        "fat-loss programming. My methods are rooted in metabolic research " +
                                        "and adapted for busy people who need time-efficient workouts.",
                    YearsOfExperience = 5,
                    Rating = 4.6m,
                    Price = 50.00m,
                    HireDate = new DateTime(2024, 3, 1)
                });
                await context.SaveChangesAsync();
            }

            // ── Trainee 1 ────────────────────────────────────────────
            var traineeUser1 = await CreateUserAsync(
                userManager,
                firstName: "Mohamed",
                lastName: "Ali",
                email: "mohamed@fitzone.com",
                password: "Trainee@1234",
                role: UserRole.Trainee,
                identityRole: "Trainee",
                photo: "images/Trainees/default.jpg"
            );

            if (traineeUser1 is not null && !await context.Trainees.AnyAsync(t => t.ApplicationUserId == traineeUser1.Id))
            {
                context.Trainees.Add(new Trainee
                {
                    ApplicationUserId = traineeUser1.Id,
                    Gender = "Male",
                    DateOfBirth = new DateTime(1998, 6, 15),
                    Weight = 82.00m,
                    Height = 178.00m,
                    Address = "Cairo"
                });
                await context.SaveChangesAsync();
            }

            // ── Trainee 2 ────────────────────────────────────────────
            var traineeUser2 = await CreateUserAsync(
                userManager,
                firstName: "Layla",
                lastName: "Ibrahim",
                email: "layla@fitzone.com",
                password: "Trainee@1234",
                role: UserRole.Trainee,
                identityRole: "Trainee",
                photo: "images/Trainees/default.jpg"
            );

            if (traineeUser2 is not null && !await context.Trainees.AnyAsync(t => t.ApplicationUserId == traineeUser2.Id))
            {
                context.Trainees.Add(new Trainee
                {
                    ApplicationUserId = traineeUser2.Id,
                    Gender = "Female",
                    DateOfBirth = new DateTime(2000, 11, 3),
                    Weight = 62.00m,
                    Height = 165.00m,
                    Address = "Alexandria"
                });
                await context.SaveChangesAsync();
            }
        }

       
        /// Creates the ApplicationUser if it doesn't exist yet and assigns it the role.
        /// If the user already exists (by email), returns the existing user — safe to call on every run.
     
        private static async Task<ApplicationUser?> CreateUserAsync(
            UserManager<ApplicationUser> userManager,
            string firstName, string lastName,
            string email, string password,
            UserRole role, string identityRole,
            string photo)
        {
            var existing = await userManager.FindByEmailAsync(email);
            if (existing is not null) return existing;

            var user = new ApplicationUser
            {
                F_Name = firstName,
                L_Name = lastName,
                UserName = $"{firstName}{lastName}".ToLower(),
                Email = email,
                Role = role,
                PhotoUrl = photo,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
                throw new Exception($"Failed to seed user {email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");

            await userManager.AddToRoleAsync(user, identityRole);
            return user;
        }
    }
}
