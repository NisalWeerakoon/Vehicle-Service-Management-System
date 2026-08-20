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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

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
        });
    }
}