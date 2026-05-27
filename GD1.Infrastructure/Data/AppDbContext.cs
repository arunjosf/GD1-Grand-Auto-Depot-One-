using GD1.Domain.Entities;
using GD1.Domain.Entities.Base;
using Microsoft.EntityFrameworkCore;

namespace GD1.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<VehicleStorageProperty> VehicleStorageProperties { get; set; }
        public DbSet<VehicleStorageSlot> VehicleStorageSlots { get; set; }
        public DbSet<LotManager> LotManagers { get; set; }
        public DbSet<StoredVehicle> StoredVehicles { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<VehicleImage> VehicleImages { get; set; }
        public DbSet<MaintenanceTask> MaintenanceTasks { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<PickupRequest> PickupRequests { get; set; }
        public DbSet<VehicleJourneyEvent> VehicleJourneyEvents { get; set; }
        public DbSet<Handoff> Handoffs { get; set; }
        public DbSet<DamageReport> DamageReports { get; set; }
        public DbSet<VehicleCatalogItem> VehicleCatalog { get; set; }
        public DbSet<GD1.Domain.Entities.FranchiseApplication> FranchiseApplications { get; set; }
        public DbSet<InspectionAssignment> InspectionAssignments { get; set; }
        public DbSet<InspectionReport> InspectionReports { get; set; }
        public DbSet<PropertyImage> PropertyImages { get; set; }
        public DbSet<Agent> Agents { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Complaint> Complaints { get; set; }
        public DbSet<TermsAndConditions> TermsAndConditions { get; set; }
        
        public DbSet<BookingAgreement> BookingAgreements { get; set; }
        public DbSet<FranchiseSlot> FranchiseSlots { get; set; }
        public DbSet<InspectionSlotItem> InspectionSlotItems { get; set; }
        public DbSet<PickupVerification> PickupVerifications { get; set; }
        public DbSet<Agreement> Agreements { get; set; }
        public DbSet<JourneyLocation> JourneyLocations { get; set; }

        protected override void OnModelCreating(ModelBuilder mb)
        {
            base.OnModelCreating(mb);
            
            mb.Entity<Agreement>().Property(a => a.Type).HasConversion<string>();
            mb.Entity<Agreement>().Property(a => a.Status).HasConversion<string>();
            
            // Fix for multiple cascade paths in StoredVehicles
            mb.Entity<StoredVehicle>()
                .HasOne(sv => sv.Property)
                .WithMany()
                .HasForeignKey(sv => sv.PropertyId)
                .OnDelete(DeleteBehavior.Restrict);

            mb.Entity<StoredVehicle>()
                .HasOne(sv => sv.Slot)
                .WithMany()
                .HasForeignKey(sv => sv.SlotId)
                .OnDelete(DeleteBehavior.Restrict);

        // ServiceCenter images removed
            mb.Entity<User>()
                .HasIndex(u => u.Email).IsUnique();
            mb.Entity<User>()
                .HasIndex(u => u.GoogleId);
            mb.Entity<Agent>().ToTable("GD1Agents")
                .HasOne(a => a.User)
                .WithOne()
                .HasForeignKey<Agent>(a => a.Id)
                .OnDelete(DeleteBehavior.Cascade);

            mb.Entity<RefreshToken>()
                .HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            mb.Entity<RefreshToken>()
                .HasIndex(r => r.Token).IsUnique();

            mb.Entity<Vehicle>()
                .HasIndex(v => v.RegistrationNo).IsUnique();
            mb.Entity<Vehicle>()
                .HasOne(v => v.Owner)
                .WithMany(u => u.Vehicles)
                .HasForeignKey(v => v.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);

            mb.Entity<VehicleImage>()
                .HasOne(vi => vi.Vehicle)
                .WithMany(v => v.Images)
                .HasForeignKey(vi => vi.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);
            mb.Entity<VehicleImage>()
                .HasOne(vi => vi.Event)
                .WithMany(e => e.Images)
                .HasForeignKey(vi => vi.EventId)
                .OnDelete(DeleteBehavior.NoAction);

            mb.Entity<VehicleJourneyEvent>()
                .HasOne(e => e.Vehicle)
                .WithMany()
                .HasForeignKey(e => e.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);
            mb.Entity<VehicleJourneyEvent>()
                .HasOne(e => e.Booking)
                .WithMany(b => b.JourneyEvents)
                .HasForeignKey(e => e.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            mb.Entity<VehicleStorageProperty>()
                .HasIndex(s => s.LotCode).IsUnique();
            mb.Entity<VehicleStorageProperty>()
                .HasOne(s => s.LotOwner)
                .WithMany()
                .HasForeignKey(s => s.LotOwnerId)
                .OnDelete(DeleteBehavior.Restrict);
            mb.Entity<VehicleStorageProperty>()
                .Property(s => s.PricePerDay).HasPrecision(10, 2);
            mb.Entity<VehicleStorageProperty>()
                .Property(s => s.AverageRating).HasPrecision(3, 2);

            mb.Entity<VehicleStorageSlot>()
                .HasOne(ls => ls.Property)
                .WithMany(p => p.Slots)
                .HasForeignKey(ls => ls.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);

            mb.Entity<LotManager>()
                .HasOne(lm => lm.Property)
                .WithMany(p => p.Managers)
                .HasForeignKey(lm => lm.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);
            mb.Entity<LotManager>()
                .HasOne(lm => lm.Manager)
                .WithMany()
                .HasForeignKey(lm => lm.ManagerId)
                .OnDelete(DeleteBehavior.Cascade);

            mb.Entity<Booking>()
                .HasOne(b => b.Vehicle)
                .WithMany(v => v.Bookings)
                .HasForeignKey(b => b.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);
            mb.Entity<Booking>()
                .HasOne(b => b.Property)
                .WithMany(p => p.Bookings)
                .HasForeignKey(b => b.PropertyId)
                .OnDelete(DeleteBehavior.Restrict);
            mb.Entity<Booking>()
                .HasOne(b => b.Owner)
                .WithMany()
                .HasForeignKey(b => b.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);
            mb.Entity<Booking>()
                .HasOne(b => b.Slot)
                .WithMany()
                .HasForeignKey(b => b.SlotId)
                .OnDelete(DeleteBehavior.Restrict);
            mb.Entity<Booking>()
                .HasIndex(b => new { b.PropertyId, b.StartDate, b.EndDate });
            mb.Entity<Booking>()
                .Property(b => b.TotalCost).HasPrecision(10, 2);
            mb.Entity<Booking>()
                .Property(b => b.PricePerDay).HasPrecision(10, 2);
            mb.Entity<Booking>()
                .Property(b => b.PlatformFee).HasPrecision(10, 2);
            mb.Entity<Booking>()
                .Property(b => b.LotEarning).HasPrecision(10, 2);

            mb.Entity<BookingAgreement>()
                .HasOne(ba => ba.Owner)
                .WithMany()
                .HasForeignKey(ba => ba.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);
            mb.Entity<BookingAgreement>()
                .HasOne(ba => ba.Vehicle)
                .WithMany()
                .HasForeignKey(ba => ba.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);
            mb.Entity<BookingAgreement>()
                .HasOne(ba => ba.Property)
                .WithMany()
                .HasForeignKey(ba => ba.PropertyId)
                .OnDelete(DeleteBehavior.Restrict);
            mb.Entity<BookingAgreement>()
                .Property(ba => ba.Status)
                .HasConversion<string>();

            mb.Entity<PickupRequest>()
                .HasOne(p => p.Booking)
                .WithMany(b => b.PickupRequests)
                .HasForeignKey(p => p.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

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

            // Service Center relationships removed

            mb.Entity<GD1.Domain.Entities.FranchiseApplication>()
                .Property(x => x.ApplicationType)
                .HasConversion<string>();

            mb.Entity<GD1.Domain.Entities.FranchiseApplication>()
                .HasOne(f => f.Applicant)
                .WithMany(u => u.FranchiseApplications)
                .HasForeignKey(f => f.ApplicantId)
                .OnDelete(DeleteBehavior.Cascade);
            mb.Entity<GD1.Domain.Entities.FranchiseApplication>()
                .Property(f => f.ApplicationFee).HasPrecision(10, 2);
            mb.Entity<GD1.Domain.Entities.FranchiseApplication>()
                .Property(f => f.PricePerDay).HasPrecision(10, 2);
            mb.Entity<GD1.Domain.Entities.FranchiseApplication>()
                .Property(f => f.Status).HasConversion<string>();

            mb.Entity<InspectionAssignment>()
                .HasOne(ia => ia.Application)
                .WithMany(a => a.Assignments)
                .HasForeignKey(ia => ia.ApplicationId)
                .OnDelete(DeleteBehavior.Cascade);
            mb.Entity<InspectionAssignment>()
                .HasOne(ia => ia.Agent)
                .WithMany(a => a.Assignments)
                .HasForeignKey(ia => ia.AgentId)
                .OnDelete(DeleteBehavior.Restrict);

            mb.Entity<InspectionReport>()
                .HasOne(ir => ir.Assignment)
                .WithOne(ia => ia.Report)
                .HasForeignKey<InspectionReport>(ir => ir.AssignmentId)
                .OnDelete(DeleteBehavior.Cascade);
            mb.Entity<InspectionReport>()
                .Property(ir => ir.AdminDecision).HasConversion<string>();

            mb.Entity<PropertyImage>()
                .HasOne(pi => pi.Application)
                .WithMany(a => a.PropertyImages)
                .HasForeignKey(pi => pi.ApplicationId)
                .OnDelete(DeleteBehavior.Cascade);

            mb.Entity<FranchiseSlot>()
                .HasOne(lus => lus.Application)
                .WithMany(a => a.Slots)
                .HasForeignKey(lus => lus.ApplicationId)
                .OnDelete(DeleteBehavior.Cascade);

            mb.Entity<InspectionSlotItem>()
                .HasOne(isi => isi.Report)
                .WithMany(r => r.SlotVerifications)
                .HasForeignKey(isi => isi.ReportId)
                .OnDelete(DeleteBehavior.Cascade);

            mb.Entity<Review>()
                .HasOne(r => r.Property)
                .WithMany(p => p.Reviews)
                .HasForeignKey(r => r.PropertyId)
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

            mb.Entity<Complaint>()
                .HasOne(c => c.Complainant)
                .WithMany()
                .HasForeignKey(c => c.ComplainantId)
                .OnDelete(DeleteBehavior.Restrict);
            mb.Entity<Complaint>()
                .HasOne(c => c.Property)
                .WithMany()
                .HasForeignKey(c => c.PropertyId)
                .OnDelete(DeleteBehavior.Restrict);
            mb.Entity<Complaint>()
                .HasOne(c => c.Booking)
                .WithMany()
                .HasForeignKey(c => c.BookingId)
                .OnDelete(DeleteBehavior.NoAction);

            mb.Entity<BookingAgreement>()
                .HasOne(a => a.Booking)
                .WithMany()
                .HasForeignKey(a => a.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            mb.Entity<PickupVerification>()
                .HasOne(p => p.Booking)
                .WithMany()
                .HasForeignKey(p => p.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            mb.Entity<MaintenanceTask>()
                .HasOne(m => m.Vehicle)
                .WithMany()
                .HasForeignKey(m => m.VehicleId)
                .OnDelete(DeleteBehavior.NoAction);

            mb.Entity<MaintenanceTask>()
                .HasOne(m => m.Booking)
                .WithMany()
                .HasForeignKey(m => m.BookingId)
                .OnDelete(DeleteBehavior.NoAction);

            mb.Entity<MaintenanceTask>()
                .HasOne(m => m.Manager)
                .WithMany()
                .HasForeignKey(m => m.ManagerId)
                .OnDelete(DeleteBehavior.NoAction);
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
