using JobMaintenanceService.Models;
using Microsoft.EntityFrameworkCore;

namespace JobMaintenanceService.Data;

public class JobMaintenanceDbContext : DbContext
{
    public JobMaintenanceDbContext(DbContextOptions<JobMaintenanceDbContext> options)
        : base(options)
    {
    }

    public DbSet<JobCard> JobCards => Set<JobCard>();
    public DbSet<ProcessedKafkaEvent> ProcessedKafkaEvents => Set<ProcessedKafkaEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<JobCard>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.JobCardNumber).IsRequired().HasMaxLength(30);
            entity.Property(x => x.VehicleRegistrationNumber).IsRequired().HasMaxLength(30);
            entity.Property(x => x.ReportedProblems).IsRequired().HasMaxLength(500);
            entity.Property(x => x.Status).IsRequired().HasMaxLength(30);

            entity.HasIndex(x => x.JobCardNumber).IsUnique();
            entity.HasIndex(x => x.CheckInId).IsUnique();
        });

        modelBuilder.Entity<ProcessedKafkaEvent>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.EventId).IsRequired();
            entity.Property(x => x.EventType).IsRequired().HasMaxLength(100);
            entity.HasIndex(x => x.EventId).IsUnique();
        });
    }
}
