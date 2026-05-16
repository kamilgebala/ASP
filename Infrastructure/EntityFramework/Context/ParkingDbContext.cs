using CoreApp.Entities;
using CoreApp.Enums;
using Infrastructure.EntityFramework.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.EntityFramework.Context;

public class ParkingDbContext : IdentityDbContext<AppUser, AppRole, string>
{
    public DbSet<ParkingGate> Gates { get; set; }
    public DbSet<ParkingSession> Sessions { get; set; }
    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<CameraCapture> Captures { get; set; }
    public DbSet<ParkingTariff> Tariffs { get; set; }

    public ParkingDbContext() { }

    public ParkingDbContext(DbContextOptions<ParkingDbContext> options) : base(options) { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
            optionsBuilder.UseSqlite("Data Source=parking.db");
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<AppUser>(e =>
        {
            e.Property(u => u.FirstName).HasMaxLength(100);
            e.Property(u => u.LastName).HasMaxLength(100);
            e.Property(u => u.FullName).HasMaxLength(200);
            e.Property(u => u.Department).HasMaxLength(100);
            e.Property(u => u.Status).HasConversion<string>();
            e.HasIndex(u => u.Email).IsUnique();
        });
        builder.Entity<AppRole>(e => e.Property(r => r.Name).HasMaxLength(50));

        builder.Entity<Vehicle>(e =>
        {
            e.Property(v => v.LicensePlate).HasMaxLength(10).IsRequired();
            e.Property(v => v.Brand).HasMaxLength(50);
            e.Property(v => v.Color).HasMaxLength(30);
            e.HasIndex(v => v.LicensePlate).IsUnique();
        });

        builder.Entity<ParkingGate>(e =>
        {
            e.Property(g => g.Name).HasMaxLength(50).IsRequired();
            e.Property(g => g.Location).HasMaxLength(100);
            e.Property(g => g.Type).HasConversion<string>();
        });

        builder.Entity<CameraCapture>(e =>
        {
            e.Property(c => c.LicensePlate).HasMaxLength(10);
            e.Property(c => c.DetectedBrand).HasMaxLength(50);
            e.Property(c => c.DetectedColor).HasMaxLength(30);
            e.Property(c => c.GateName).HasMaxLength(50);
            e.Property(c => c.Type).HasConversion<string>();
        });

        builder.Entity<CameraCapture>()
            .HasOne(c => c.Gate)
            .WithMany(g => g.CameraCaptures)
            .HasForeignKey(c => c.GateId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ParkingSession>(e =>
        {
            e.Property(s => s.GateName).HasMaxLength(50);
        });

        builder.Entity<ParkingSession>()
            .HasOne(s => s.Vehicle)
            .WithMany()
            .HasForeignKey(s => s.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ParkingTariff>(e =>
        {
            e.Property(t => t.Name).HasMaxLength(50).IsRequired();
            e.Property(t => t.FreeParkingDuration)
                .HasConversion(v => v.Ticks, v => TimeSpan.FromTicks(v));
        });
    }
}