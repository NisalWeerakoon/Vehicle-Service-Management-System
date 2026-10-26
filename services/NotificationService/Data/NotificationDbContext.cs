using Microsoft.EntityFrameworkCore;
using NotificationService.Models;

namespace NotificationService.Data;

public class NotificationDbContext(DbContextOptions<NotificationDbContext> options) : DbContext(options)
{
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<ProcessedKafkaEvent> ProcessedKafkaEvents => Set<ProcessedKafkaEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Title).IsRequired().HasMaxLength(200);
            entity.Property(x => x.Message).IsRequired().HasMaxLength(1000);
            entity.Property(x => x.Type).IsRequired().HasMaxLength(80);
            entity.HasIndex(x => new { x.RecipientUserId, x.IsRead });
            entity.HasIndex(x => new { x.RecipientUserId, x.CreatedAt });
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
