using Microsoft.EntityFrameworkCore;
using VisitorManagementSystem.Models;

namespace VisitorManagementSystem.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Visitor> Visitors { get; set; }
        public DbSet<Resident> Residents { get; set; }
        public DbSet<ResidentContact> ResidentContacts { get; set; }
        public DbSet<ResidentVehicle> ResidentVehicles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<VisitorLog> VisitorLogs { get; set; }
        public DbSet<MailSettings> MailSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Visitor configuration
            modelBuilder.Entity<Visitor>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ApartmentNumber).IsRequired().HasMaxLength(10);
                entity.Property(e => e.LicensePlate).HasMaxLength(20);
                entity.Property(e => e.IdNumber).HasMaxLength(20);
                entity.Property(e => e.ResidentPhone).HasMaxLength(15);
                entity.Property(e => e.ResidentName).HasMaxLength(100);
                entity.Property(e => e.VisitorPhone).HasMaxLength(15);
                entity.Property(e => e.Notes).HasMaxLength(500);
                entity.HasIndex(e => e.ApartmentNumber);
                entity.HasIndex(e => e.CheckInTime);
            });

            // Resident configuration
            modelBuilder.Entity<Resident>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ApartmentNumber).IsRequired().HasMaxLength(10);
                entity.Property(e => e.Block).HasMaxLength(5);
                entity.Property(e => e.SubBlock).HasMaxLength(5);
                entity.Property(e => e.DoorNumber).HasMaxLength(10);
                entity.Property(e => e.Notes).HasMaxLength(200);
                entity.HasIndex(e => e.ApartmentNumber).IsUnique();
                entity.HasIndex(e => new { e.Block, e.SubBlock, e.DoorNumber });
                
                // Relationships
                entity.HasMany(e => e.Contacts)
                      .WithOne(c => c.Resident)
                      .HasForeignKey(c => c.ResidentId)
                      .OnDelete(DeleteBehavior.Cascade);
                      
                entity.HasMany(e => e.Vehicles)
                      .WithOne(v => v.Resident)
                      .HasForeignKey(v => v.ResidentId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
            
            // ResidentContact configuration
            modelBuilder.Entity<ResidentContact>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ContactType).IsRequired().HasMaxLength(20);
                entity.Property(e => e.ContactValue).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Label).HasMaxLength(50);
                entity.HasIndex(e => e.ContactValue);
                entity.HasIndex(e => new { e.ResidentId, e.ContactType, e.Priority });
            });
            
            // ResidentVehicle configuration
            modelBuilder.Entity<ResidentVehicle>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.LicensePlate).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Brand).HasMaxLength(50);
                entity.Property(e => e.Model).HasMaxLength(50);
                entity.Property(e => e.Color).HasMaxLength(20);
                entity.Property(e => e.Year).HasMaxLength(4);
                entity.Property(e => e.VehicleType).HasMaxLength(50);
                entity.Property(e => e.Notes).HasMaxLength(200);
                entity.HasIndex(e => e.LicensePlate);
                entity.HasIndex(e => e.ResidentId);
            });

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Role).IsRequired().HasMaxLength(20);
                entity.HasIndex(e => e.Username).IsUnique();
            });

            // VisitorLog configuration
            modelBuilder.Entity<VisitorLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Action).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Details).HasMaxLength(200);
                entity.HasOne(e => e.Visitor)
                      .WithMany()
                      .HasForeignKey(e => e.VisitorId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => e.VisitorId);
            });

            // MailSettings configuration
            modelBuilder.Entity<MailSettings>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.SenderName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.SenderEmail).IsRequired().HasMaxLength(100);
                entity.Property(e => e.SmtpServer).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Password).IsRequired();
                entity.Property(e => e.SecurityType).IsRequired().HasMaxLength(10);
                entity.Property(e => e.CreatedBy).HasMaxLength(50);
                entity.Property(e => e.UpdatedBy).HasMaxLength(50);
            });

            // Seed data removed - handled dynamically in Program.cs
        }
    }
}