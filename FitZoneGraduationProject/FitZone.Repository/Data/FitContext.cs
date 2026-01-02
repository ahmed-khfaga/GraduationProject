using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FitZone.Core.Entitys;
using Microsoft.EntityFrameworkCore;

namespace FitZone.Repository.Data
{
    public class FitContext : DbContext
    {
        public FitContext(DbContextOptions<FitContext> option):base(option)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
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



        }


        public virtual DbSet<User> Users { get; set; }

        public virtual DbSet<Trainee> Trainees { get; set; }

        public virtual DbSet<Coach> Coachs { get; set; }

        public virtual DbSet<Membership> Memberships { get; set; }

        public virtual DbSet<MembershipPlan> MembershipPlans { get; set; }

        public virtual DbSet<TraineeMembership> TraineeMemberships { get; set; }
    }
}
