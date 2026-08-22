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
    }
}