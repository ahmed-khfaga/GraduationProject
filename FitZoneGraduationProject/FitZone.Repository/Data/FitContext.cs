using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FitZone.Core.Entitys;
using FitZone.Core.Entitys.Identity;
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
                .HasForeignKey(e => e.CoachID)
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
                .HasIndex(x => new { x.TraineeID, x.TrackID, x.IsActive })
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
                    .HasForeignKey(pt => pt.CoachID)
                    .OnDelete(DeleteBehavior.NoAction);


            modelBuilder.Entity<TraineeProgramEnrollment>()
                    .HasOne(e => e.Trainee)
                    .WithMany(t => t.TraineeProgramEnrollments)
                    .HasForeignKey(e => e.TraineeID)
                    .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<TraineeProgramEnrollment>()
                    .HasOne(e => e.WorkoutProgram)
                    .WithMany(w => w.TraineeProgramEnrollments)
                    .HasForeignKey(e => e.WorkoutProgramID)
                    .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<TraineeProgramEnrollment>()
                    .HasOne(e => e.Track)
                    .WithMany()
                    .HasForeignKey(e => e.TrackID)
                    .OnDelete(DeleteBehavior.NoAction);


            #endregion



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




    }
}
