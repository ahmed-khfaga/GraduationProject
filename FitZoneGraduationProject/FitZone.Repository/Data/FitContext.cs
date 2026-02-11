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

            modelBuilder.Entity<Coach>()
                .Property(c => c.Price)
                .HasPrecision(3,2);

            modelBuilder.Entity<Coach>()
                .Property(c => c.Rating)
                .HasPrecision(2, 1);

            modelBuilder.Entity<Coach>() // to max rate is 0 to 5 no more no less 
                .ToTable(t => t.HasCheckConstraint(
                    "CK_Coach_Rating",
                    "[Rating] >= 0 AND [Rating] <= 5"
                ));

            modelBuilder.Entity<MembershipPlan>()
                 .Property(m => m.Price)
                 .HasPrecision(5, 2);

            modelBuilder.Entity<Trainee>()
                .Property(t => t.Weight)
                .HasPrecision(3, 2);

            modelBuilder.Entity<Trainee>()
                .Property(t => t.Height)
                .HasPrecision(3, 2);

            modelBuilder.Entity<TraineeProgramTemplate>() // Only ONE active template at a time
                .HasIndex(x => new { x.TraineeID, x.IsActive })
                .IsUnique()
                .HasFilter("[IsActive] = 1");

            modelBuilder.Entity<Trainee>()
                .HasOne(t => t.ApplicationUser)
                .WithOne(u => u.Trainee)
                .HasForeignKey<Trainee>(t => t.ApplicationUserId);

            modelBuilder.Entity<Coach>()
                .HasOne(c => c.ApplicationUser)
                .WithOne(u => u.Coach)
                .HasForeignKey<Coach>(c => c.ApplicationUserId);



            #region NoAction on Deleted Coach
            modelBuilder.Entity<ProgramTemplate>()
                    .HasOne(pt => pt.Coach)
                    .WithMany(c => c.ProgramTemplates)
                    .HasForeignKey(pt => pt.CoachID)
                    .OnDelete(DeleteBehavior.NoAction);

            #endregion



        }

        public virtual DbSet<Trainee> Trainees { get; set; }

        public virtual DbSet<Coach> Coachs { get; set; }

        public virtual DbSet<Membership> Memberships { get; set; }

        public virtual DbSet<MembershipPlan> MembershipPlans { get; set; }

        public virtual DbSet<BaseProgram> BasePrograms { get; set; }

        public virtual DbSet<ProgramTemplate> ProgramTemplates { get; set; }

        public virtual DbSet<ProgramDays> ProgramDays { get; set; }

        public virtual DbSet<TraineeProgramTemplate> TraineeProgramTemplates { get; set; }
        public virtual DbSet<TraineeMembership> TraineeMemberships { get; set; }


    }
}
