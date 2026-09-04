using CustomerBookingService.Models;
using Microsoft.EntityFrameworkCore;

namespace CustomerBookingService.Data;

public class CustomerBookingDbContext : DbContext
{
    public CustomerBookingDbContext(
        DbContextOptions<CustomerBookingDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<Vehicle> Vehicles => Set<Vehicle>();

    public DbSet<Booking> Bookings => Set<Booking>();

    public DbSet<CheckIn> CheckIns => Set<CheckIn>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ======================================================
        // USER
        // ======================================================

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);

            entity.HasIndex(u => u.Email)
                .IsUnique();

            entity.Property(u => u.Email)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(u => u.PasswordHash)
                .IsRequired();

            entity.Property(u => u.Role)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(u => u.IsActive)
                .HasDefaultValue(true);

            entity.HasOne(u => u.Customer)
                .WithOne()
                .HasForeignKey<User>(u => u.CustomerId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // ======================================================
        // CUSTOMER
        // ======================================================

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(c => c.Id);

            entity.HasIndex(c => c.Email)
                .IsUnique();

            entity.Property(c => c.FullName)
                .HasMaxLength(120)
                .IsRequired();

            entity.Property(c => c.Email)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(c => c.Phone)
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(c => c.Address)
                .HasMaxLength(250);
        });

        // ======================================================
        // VEHICLE
        // ======================================================

        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.HasKey(v => v.Id);

            entity.HasIndex(v => v.RegistrationNumber)
                .IsUnique();

            entity.Property(v => v.RegistrationNumber)
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(v => v.Make)
                .HasMaxLength(80)
                .IsRequired();

            entity.Property(v => v.Model)
                .HasMaxLength(80)
                .IsRequired();

            entity.Property(v => v.FuelType)
                .HasMaxLength(40)
                .IsRequired();

            entity.HasOne(v => v.Customer)
                .WithMany(c => c.Vehicles)
                .HasForeignKey(v => v.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ======================================================
        // BOOKING
        // ======================================================

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(b => b.Id);

            entity.HasIndex(b => b.BookingReference)
                .IsUnique();

            entity.Property(b => b.BookingReference)
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(b => b.RequestedServiceOrProblem)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(b => b.Status)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            entity.HasOne(b => b.Customer)
                .WithMany(c => c.Bookings)
                .HasForeignKey(b => b.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(b => b.Vehicle)
                .WithMany(v => v.Bookings)
                .HasForeignKey(b => b.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);
        });


        // ======================================================
        // CHECK-IN
        // ======================================================

        modelBuilder.Entity<CheckIn>(entity =>
        {
            entity.HasKey(c => c.Id);

            entity.Property(c => c.ReportedProblems)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(c => c.IsActive)
                .HasDefaultValue(true);

            entity.HasOne(c => c.Booking)
                .WithMany()
                .HasForeignKey(c => c.BookingId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(c => c.Customer)
                .WithMany()
                .HasForeignKey(c => c.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(c => c.Vehicle)
                .WithMany()
                .HasForeignKey(c => c.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(c => new { c.BookingId, c.IsActive });
            entity.HasIndex(c => new { c.VehicleId, c.IsActive });
        });
    }
}