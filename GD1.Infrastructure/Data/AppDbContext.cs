using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GD1.Domain.Entities;
using GD1.Domain.Entities.Base;

namespace GD1.Infrastructure.Data
{
   public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<StorageLot> StorageLots { get; set; }
        public DbSet<LotSlot> LotSlots { get; set; }
        public DbSet<LotManager> LotManagers { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<VehicleJourneyEvent> VehicleJourneyEvents { get; set; }
        public DbSet<Handoff> Handoffs { get; set; }
        public DbSet<DamageReport> DamageReports { get; set; }
        public DbSet<ServiceCenter> ServiceCenters { get; set; }
        public DbSet<Mechanics> Mechanics { get; set; }
        public DbSet<ServiceRequest> ServiceRequests { get; set; }
        public DbSet<FranchiseApplication> FranchiseApplications { get; set; }
        public DbSet<LotUnit> LotUnits { get; set; }
        public DbSet<InspectionReport> InspectionReports { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder mb)
        {

            mb.Entity<User>()
                .HasIndex(u => u.Email).IsUnique();

            mb.Entity<User>()
                .HasIndex(u => u.GoogleId);

            mb.Entity<RefreshToken>()
                .HasOne(rt => rt.user)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            mb.Entity<Vehicle>()
                .HasIndex(v => v.RegistrationNo).IsUnique();

            mb.Entity<Vehicle>()
                .HasOne(v => v.Owner)
                .WithMany(u => u.Vehicles)
                .HasForeignKey(v => v.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);

            mb.Entity<StorageLot>()
                .HasIndex(s => s.LotCode).IsUnique();

            mb.Entity<StorageLot>()
                .HasOne(s => s.LotOwner)
                .WithMany()
                .HasForeignKey(s => s.LotOwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            mb.Entity<LotSlot>()
                .HasOne(ls => ls.Lot)
                .WithMany(l => l.Slots)
                .HasForeignKey(ls => ls.LotId)
                .OnDelete(DeleteBehavior.Cascade);

            mb.Entity<LotManager>()
                .HasOne(lm => lm.LotOwner)
                .WithMany(l => l.Managers)
                .HasForeignKey(lm => lm.LotId)
                .OnDelete(DeleteBehavior.Cascade);

            mb.Entity<LotManager>()
                .HasOne(lm => lm.Manager)
                .WithMany()
                .HasForeignKey(lm => lm.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);


            mb.Entity<Booking>()
                .HasOne(b => b.Vehicle)
                .WithMany(v => v.Bookings)
                .HasForeignKey(b => b.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            mb.Entity<Booking>()
                .HasOne(b => b.Lot)
                .WithMany(l => l.Bookings)
                .HasForeignKey(b => b.LotId)
                .OnDelete(DeleteBehavior.Restrict);

            mb.Entity<Booking>()
                .HasOne(b => b.Owner)
                .WithMany()
                .HasForeignKey(b => b.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            mb.Entity<Booking>()
                .HasIndex(b => new { b.LotId, b.StartDate, b.EndDate });

            mb.Entity<Handoff>()
                .HasOne(h => h.Booking)
                .WithMany(b => b.Handoffs)
                .HasForeignKey(h => h.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            mb.Entity<DamageReport>()
                .HasOne(d => d.Handoff)
                .WithOne(h => h.DamageReport)
                .HasForeignKey<DamageReport>(d => d.HandoffId)
                .OnDelete(DeleteBehavior.Cascade);


            mb.Entity<ServiceCenter>()
                .HasOne(sc => sc.ServiceCenterAdmin)
                .WithMany()
                .HasForeignKey(sc => sc.AdminId)
                .OnDelete(DeleteBehavior.Restrict);


            mb.Entity<Mechanics>()
                .HasOne(m => m.ServiceCenter)
                .WithMany(sc => sc.Mechanics)
                .HasForeignKey(m => m.ServiceCenterId)
                .OnDelete(DeleteBehavior.Cascade);

            mb.Entity<ServiceRequest>()
                .HasOne(sr => sr.Booking)
                .WithMany(b => b.ServiceRequests)
                .HasForeignKey(sr => sr.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            mb.Entity<ServiceRequest>()
                .HasOne(sr => sr.ServiceCenter)
                .WithMany(sc => sc.ServiceRequests)
                .HasForeignKey(sr => sr.ServiceCenterId)
                .OnDelete(DeleteBehavior.Restrict);

            mb.Entity<FranchiseApplication>()
                .HasOne(f => f.Applicant)
                .WithMany(u => u.FranchiseApplications)
                .HasForeignKey(f => f.ApplicantId)
                .OnDelete(DeleteBehavior.Cascade);


            mb.Entity<LotUnit>()
                .HasOne(l => l.Application)
                .WithMany(a => a.LotUnits)
                .HasForeignKey(l => l.FranchiseApplicationId)
                .OnDelete(DeleteBehavior.Cascade);

            mb.Entity<InspectionReport>()
                .HasOne(ir => ir.Application)
                .WithMany(a => a.InspectionReports)
                .HasForeignKey(ir => ir.ApplicationId)
                .OnDelete(DeleteBehavior.Restrict);

            mb.Entity<InspectionReport>()
                .HasOne(ir => ir.LotUnit)
                .WithMany(l => l.InspectionReports)
                .HasForeignKey(ir => ir.LotUnitId)
                .OnDelete(DeleteBehavior.Cascade);

            mb.Entity<Review>()
                .HasOne(r => r.Lot)
                .WithMany(l => l.Reviews)
                .HasForeignKey(r => r.LotId)
                .OnDelete(DeleteBehavior.Cascade);

            mb.Entity<Review>()
                .HasOne(r => r.Reviewer)
                .WithMany()
                .HasForeignKey(r => r.ReviewerId)
                .OnDelete(DeleteBehavior.Restrict);


            mb.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            mb.Entity<StorageLot>().Property(s => s.PricePerDay).HasPrecision(10, 2);
            mb.Entity<StorageLot>().Property(s => s.AverageRating).HasPrecision(3, 2);

            mb.Entity<Booking>().Property(b => b.TotalCost).HasPrecision(10, 2);
            mb.Entity<Booking>().Property(b => b.PlatformFee).HasPrecision(10, 2);
            mb.Entity<Booking>().Property(b => b.LotEarning).HasPrecision(10, 2);

            mb.Entity<ServiceRequest>().Property(s => s.ServiceCost).HasPrecision(10, 2);
        }


        public override Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                if (entry.State == EntityState.Modified)
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
            }

            return base.SaveChangesAsync(ct);
        }
    }
}
