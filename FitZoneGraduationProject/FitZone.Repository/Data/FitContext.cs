using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FitZone.Core.Entitys;
using FitZone.Core.Entitys.Chat;
using FitZone.Core.Entitys.Identity;
using FitZone.Core.Entitys.PaymentEntity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FitZone.Repository.Data
{
    public class FitContext : IdentityDbContext<ApplicationUser>
    {
        public FitContext(DbContextOptions<FitContext> option):base(option)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // called base onModelCreating to not breake Identity configuration .

            // ── Coach
            modelBuilder.Entity<Coach>()
                .Property(c => c.Price)
                .HasColumnType("decimal(8,2)");

            modelBuilder.Entity<Coach>()
                .Property(c => c.Rating)
                .HasColumnType("decimal(3,2)");

            modelBuilder.Entity<Coach>() // to max rate is 0 to 5 no more no less 
                .ToTable(t => t.HasCheckConstraint(
                    "CK_Coach_Rating",
                    "[Rating] >= 0 AND [Rating] <= 5"
                ));

            //  Exercise — optional coach ownership 
            modelBuilder.Entity<Exercise>()
                .HasOne(e => e.Coach)
                .WithMany(c => c.Exercises)
                .HasForeignKey(e => e.CoachId)
                .OnDelete(DeleteBehavior.SetNull); // deleting a coach nullifies ownership, keeps exercise data intact
           
            
            // ── MembershipPlan
            modelBuilder.Entity<MembershipPlan>()
                 .Property(m => m.Price)
                 .HasColumnType("decimal(8,2)");

            // ── Trainee
            modelBuilder.Entity<Trainee>()
                .Property(t => t.Weight)
                .HasColumnType("decimal(6,2)");

            modelBuilder.Entity<Trainee>()
                .Property(t => t.Height)
                .HasColumnType("decimal(6,2)");

            // ── Enrollment constraints, only active one program per track at a time.
            modelBuilder.Entity<TraineeProgramEnrollment>()
                .HasIndex(x => new { x.TraineeId, x.TrackId, x.IsActive })
                .IsUnique()
                .HasFilter("[IsActive] = 1");

            // ── Identity relationships
            modelBuilder.Entity<Trainee>()
                .HasOne(t => t.ApplicationUser)
                .WithOne(u => u.Trainee)
                .HasForeignKey<Trainee>(t => t.ApplicationUserId);

            modelBuilder.Entity<Coach>()
                .HasOne(c => c.ApplicationUser)
                .WithOne(u => u.Coach)
                .HasForeignKey<Coach>(c => c.ApplicationUserId);



            #region NoAction on Deleted Coach
            modelBuilder.Entity<WorkoutProgram>()
                    .HasOne(pt => pt.Coach)
                    .WithMany(c => c.WorkoutPrograms)
                    .HasForeignKey(pt => pt.CoachId)
                    .OnDelete(DeleteBehavior.NoAction);


            modelBuilder.Entity<TraineeProgramEnrollment>()
                    .HasOne(e => e.Trainee)
                    .WithMany(t => t.TraineeProgramEnrollments)
                    .HasForeignKey(e => e.TraineeId)
                    .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<TraineeProgramEnrollment>()
                    .HasOne(e => e.WorkoutProgram)
                    .WithMany(w => w.TraineeProgramEnrollments)
                    .HasForeignKey(e => e.WorkoutProgramId)
                    .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<TraineeProgramEnrollment>()
                    .HasOne(e => e.Track)
                    .WithMany()
                    .HasForeignKey(e => e.TrackId)
                    .OnDelete(DeleteBehavior.NoAction);


            #endregion

            #region ChatUser
            modelBuilder.Entity<ChatMessage>(entity =>
            {
                entity.HasOne(m => m.Sender)
                      .WithMany()
                      .HasForeignKey(m => m.SenderId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(m => m.Receiver)
                      .WithMany()
                      .HasForeignKey(m => m.ReceiverId)
                      .IsRequired(false)                       // nullable for bot rows
                      .OnDelete(DeleteBehavior.Restrict);

                entity.Property(m => m.ChatType)
                  .HasConversion<int>()
                  .HasDefaultValue(ChatType.HumanToHuman);

                entity.Property(m => m.BotConversationId).IsRequired(false);

                entity.Property(m => m.BotRole)
                      .HasMaxLength(16)
                      .IsRequired(false);

                entity.HasIndex(m => new { m.SenderId, m.BotConversationId })
                      .HasDatabaseName("IX_ChatMessages_BotSession")
                      .HasFilter("[BotConversationId] IS NOT NULL");

                entity.HasIndex(m => new { m.SenderId, m.ReceiverId, m.ChatType })
                      .HasDatabaseName("IX_ChatMessages_HumanChat");
            });
            #endregion

            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasOne(p => p.User)
                      .WithMany(u => u.Payments) 
                      .HasForeignKey(p => p.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.MembershipPlan)
                      .WithMany(m => m.Payments)
                      .HasForeignKey(p => p.MembershipPlanId)
                      .OnDelete(DeleteBehavior.Restrict);
            });


            // ── NutritionPlan ────────────────────────────────────────────────
            // Coach owns many plans (same NoAction pattern as WorkoutProgram → Coach
            // to prevent cascade cycles when the coach entity is involved).
            modelBuilder.Entity<NutritionPlan>()
                .HasOne(n => n.Coach)
                .WithMany()
                .HasForeignKey(n => n.CoachID)
                .OnDelete(DeleteBehavior.NoAction);

            // Optional link to a WorkoutProgram (bundle).
            // NoAction: deleting the program does NOT delete the nutrition plan.
            modelBuilder.Entity<NutritionPlan>()
                .HasOne(n => n.LinkedWorkoutProgram)
                .WithMany()
                .HasForeignKey(n => n.LinkedWorkoutProgramID)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            // ── NutritionWeek ─────────────────────────────────────────────────
            // Cascade: deleting a plan removes all its weeks.
            modelBuilder.Entity<NutritionWeek>()
                .HasOne(w => w.NutritionPlan)
                .WithMany(p => p.NutritionWeeks)
                .HasForeignKey(w => w.NutritionPlanID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<NutritionWeek>()
                .Property(w => w.CalorieModifier)
                .HasColumnType("decimal(5,4)");  // e.g. -0.1000 to +0.1000

            // ── DayProtocol ───────────────────────────────────────────────────
            // Cascade: deleting a week removes all its day protocols.
            modelBuilder.Entity<DayProtocol>()
                .HasOne(d => d.NutritionWeek)
                .WithMany(w => w.DayProtocols)
                .HasForeignKey(d => d.NutritionWeekID)
                .OnDelete(DeleteBehavior.Cascade);

            // Optional link to a WorkoutSession (for meal timing context).
            // NoAction: deleting a session does NOT delete the day protocol.
            modelBuilder.Entity<DayProtocol>()
                .HasOne(d => d.LinkedWorkoutSession)
                .WithMany()
                .HasForeignKey(d => d.LinkedWorkoutSessionID)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            // ── Meal ──────────────────────────────────────────────────────────
            // Cascade: deleting a DayProtocol removes all its meals.
            modelBuilder.Entity<Meal>()
                .HasOne(m => m.DayProtocol)
                .WithMany(d => d.Meals)
                .HasForeignKey(m => m.DayProtocolID)
                .OnDelete(DeleteBehavior.Cascade);

            // ── MealFoodItem ──────────────────────────────────────────────────
            // Cascade from Meal: removing a meal removes its food assignments.
            modelBuilder.Entity<MealFoodItem>()
                .HasOne(mf => mf.Meal)
                .WithMany(m => m.MealFoodItems)
                .HasForeignKey(mf => mf.MealID)
                .OnDelete(DeleteBehavior.Cascade);

            // NoAction from FoodItem: removing a food item does NOT cascade to assignments.
            // The food item must be unassigned before deletion (service-layer check).
            modelBuilder.Entity<MealFoodItem>()
                .HasOne(mf => mf.FoodItem)
                .WithMany(f => f.MealFoodItems)
                .HasForeignKey(mf => mf.FoodItemID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<MealFoodItem>()
                .Property(mf => mf.AmountGrams)
                .HasColumnType("decimal(7,2)");

            // ── FoodItem ──────────────────────────────────────────────────────
            // SetNull on CoachID: same pattern as Exercise.
            // Deleting a coach sets their private food items' CoachID to null,
            // converting them to global items — data is preserved, ownership removed.
            modelBuilder.Entity<FoodItem>()
                .HasOne(f => f.Coach)
                .WithMany()
                .HasForeignKey(f => f.CoachID)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<FoodItem>()
                .Property(f => f.CaloriesPer100g)
                .HasColumnType("decimal(7,2)");

            modelBuilder.Entity<FoodItem>()
                .Property(f => f.ProteinPer100g)
                .HasColumnType("decimal(7,2)");

            modelBuilder.Entity<FoodItem>()
                .Property(f => f.CarbPer100g)
                .HasColumnType("decimal(7,2)");

            modelBuilder.Entity<FoodItem>()
                .Property(f => f.FatPer100g)
                .HasColumnType("decimal(7,2)");

            modelBuilder.Entity<FoodItem>()
                .Property(f => f.FiberPer100g)
                .HasColumnType("decimal(7,2)");

            // ── TraineeNutritionEnrollment ────────────────────────────────────
            // NoAction on all three FKs to prevent cascade cycles
            // (same reasoning as TraineeProgramEnrollment in the training system).

            modelBuilder.Entity<TraineeNutritionEnrollment>()
                .HasOne(e => e.Trainee)
                .WithMany()
                .HasForeignKey(e => e.TraineeID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<TraineeNutritionEnrollment>()
                .HasOne(e => e.NutritionPlan)
                .WithMany(p => p.TraineeNutritionEnrollments)
                .HasForeignKey(e => e.NutritionPlanID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<TraineeNutritionEnrollment>()
                .HasOne(e => e.LinkedWorkoutEnrollment)
                .WithMany()
                .HasForeignKey(e => e.LinkedWorkoutEnrollmentID)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.NoAction);

            // Partial unique index: one active enrollment per NutritionPlan per Trainee.
            // Matches the partial index pattern used on TraineeProgramEnrollment.
            // Allows multiple historical (inactive) rows per plan+trainee combination
            // while guaranteeing at most one active row at DB level.
            modelBuilder.Entity<TraineeNutritionEnrollment>()
                .HasIndex(e => new { e.TraineeID, e.NutritionPlanID, e.IsActive })
                .IsUnique()
                .HasFilter("[IsActive] = 1");

            // ── ClientNutritionConstraints ────────────────────────────────────
            // One-to-one with TraineeNutritionEnrollment.
            // Cascade: if the enrollment is ever hard-deleted, its constraints go too.
            modelBuilder.Entity<ClientNutritionConstraints>()
                .HasOne(c => c.Enrollment)
                .WithOne(e => e.Constraints)
                .HasForeignKey<ClientNutritionConstraints>(c => c.EnrollmentID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ClientNutritionConstraints>()
                .Property(c => c.ExpectedWeeklyChangeMin)
                .HasColumnType("decimal(5,3)");

            modelBuilder.Entity<ClientNutritionConstraints>()
                .Property(c => c.ExpectedWeeklyChangeMax)
                .HasColumnType("decimal(5,3)");

            modelBuilder.Entity<ClientNutritionConstraints>()
                .Property(c => c.DeviationTriggerKg)
                .HasColumnType("decimal(5,3)");

            // DB-level check: adherence is always between 0 and 100.
            modelBuilder.Entity<ClientNutritionConstraints>()
                .ToTable(t => t.HasCheckConstraint(
                    "CK_Constraints_Adherence",
                    "[AdherenceThresholdPercent] >= 0 AND [AdherenceThresholdPercent] <= 100"));

            // DB-level check: weight averaging days must be 3, 5, or 7.
            modelBuilder.Entity<ClientNutritionConstraints>()
                .ToTable(t => t.HasCheckConstraint(
                    "CK_Constraints_WeightAveragingDays",
                    "[WeightAveragingDays] IN (3, 5, 7)"));

            // ── WeeklyCheckIn ─────────────────────────────────────────────────
            // NoAction: preserves check-in history even if enrollment is cancelled.
            modelBuilder.Entity<WeeklyCheckIn>()
                .HasOne(c => c.Enrollment)
                .WithMany(e => e.WeeklyCheckIns)
                .HasForeignKey(c => c.EnrollmentID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<WeeklyCheckIn>()
                .Property(c => c.MorningWeight1)
                .HasColumnType("decimal(6,2)");

            modelBuilder.Entity<WeeklyCheckIn>()
                .Property(c => c.MorningWeight2)
                .HasColumnType("decimal(6,2)");

            modelBuilder.Entity<WeeklyCheckIn>()
                .Property(c => c.MorningWeight3)
                .HasColumnType("decimal(6,2)");

            modelBuilder.Entity<WeeklyCheckIn>()
                .Property(c => c.AverageWeight)
                .HasColumnType("decimal(6,2)");

            // DB-level check: scale values are 1–5.
            modelBuilder.Entity<WeeklyCheckIn>()
                .ToTable(t => t.HasCheckConstraint(
                    "CK_CheckIn_EnergyLevel",
                    "[EnergyLevel] >= 1 AND [EnergyLevel] <= 5"));

            modelBuilder.Entity<WeeklyCheckIn>()
                .ToTable(t => t.HasCheckConstraint(
                    "CK_CheckIn_HungerLevel",
                    "[HungerLevel] >= 1 AND [HungerLevel] <= 5"));

            modelBuilder.Entity<WeeklyCheckIn>()
                .ToTable(t => t.HasCheckConstraint(
                    "CK_CheckIn_SleepQuality",
                    "[SleepQuality] >= 1 AND [SleepQuality] <= 5"));

            // DB-level check: adherence 0–100.
            modelBuilder.Entity<WeeklyCheckIn>()
                .ToTable(t => t.HasCheckConstraint(
                    "CK_CheckIn_Adherence",
                    "[AdherencePercent] >= 0 AND [AdherencePercent] <= 100"));

            // Unique index: one check-in per week per enrollment.
            modelBuilder.Entity<WeeklyCheckIn>()
                .HasIndex(c => new { c.EnrollmentID, c.WeekNumber })
                .IsUnique();

            // ClientNote length enforced at DB level as well as DTO validation.
            modelBuilder.Entity<WeeklyCheckIn>()
                .Property(c => c.ClientNote)
                .HasMaxLength(400);

            // CoachNote length enforced at DB level.
            modelBuilder.Entity<WeeklyCheckIn>()
                .Property(c => c.CoachNote)
                .HasMaxLength(500);
        }

        public virtual DbSet<Trainee> Trainees { get; set; }

        public virtual DbSet<Coach> Coachs { get; set; }

        public virtual DbSet<Exercise> Exercises { get; set; }

        public virtual DbSet<ProgramWeek> ProgramWeeks { get; set; }

        public virtual DbSet<SessionExercise> SessionExercises { get; set; }


        public virtual DbSet<Membership> Memberships { get; set; }

        public virtual DbSet<MembershipPlan> MembershipPlans { get; set; }

        public virtual DbSet<Track> Tracks { get; set; }

        public virtual DbSet<WorkoutProgram> WorkoutPrograms { get; set; }

        public virtual DbSet<WorkoutSession> WorkoutSessions { get; set; }

        public virtual DbSet<TraineeProgramEnrollment> TraineeProgramEnrollments { get; set; }

        public virtual DbSet<TraineeMembership> TraineeMemberships { get; set; }

        public virtual DbSet<ChatMessage> ChatMessages { get; set; }

        public virtual DbSet<Payment> Payments { get; set; }
        //____
        public virtual DbSet<NutritionPlan> NutritionPlans { get; set; }
        public virtual DbSet<NutritionWeek> NutritionWeeks { get; set; }
        public virtual DbSet<DayProtocol> DayProtocols { get; set; }
        public virtual DbSet<Meal> Meals { get; set; }
        public virtual DbSet<MealFoodItem> MealFoodItems { get; set; }
        public virtual DbSet<FoodItem> FoodItems { get; set; }
        public virtual DbSet<TraineeNutritionEnrollment> TraineeNutritionEnrollments { get; set; }
        public virtual DbSet<ClientNutritionConstraints> ClientNutritionConstraints { get; set; }
        public virtual DbSet<WeeklyCheckIn> WeeklyCheckIns { get; set; }



    }
}
