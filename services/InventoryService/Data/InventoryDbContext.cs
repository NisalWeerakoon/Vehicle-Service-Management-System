using InventoryService.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Data
{
    public class InventoryDbContext : DbContext
    {
        public InventoryDbContext(
            DbContextOptions<InventoryDbContext> options)
            : base(options)
        {
        }

        public DbSet<SparePart> SpareParts => Set<SparePart>();
        public DbSet<PartRequest> PartRequests => Set<PartRequest>();
        public DbSet<PartIssue> PartIssues => Set<PartIssue>();
        public DbSet<ProcessedKafkaEvent> ProcessedKafkaEvents => Set<ProcessedKafkaEvent>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<SparePart>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Name).IsRequired().HasMaxLength(200);
                entity.Property(x => x.Description).IsRequired().HasMaxLength(1000);
                entity.Property(x => x.Quantity).IsRequired();
                entity.Property(x => x.LowStockThreshold).IsRequired();
                entity.Property(x => x.UnitPrice).HasPrecision(18, 2).IsRequired();
                entity.HasIndex(x => x.Name);
            });

            modelBuilder.Entity<PartRequest>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Status).IsRequired().HasMaxLength(20);
                entity.Property(x => x.RequestingMechanicId).IsRequired().HasMaxLength(100);
                entity.Property(x => x.RequestingMechanicName).IsRequired().HasMaxLength(150);
                entity.Property(x => x.JobCardNumber).HasMaxLength(30);
                entity.HasIndex(x => new { x.Status, x.RequestedAt });
                entity.HasIndex(x => x.SourceRequestId).IsUnique();
                entity.HasIndex(x => x.JobCardId);
                entity.HasOne(x => x.SparePart).WithMany().HasForeignKey(x => x.SparePartId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<PartIssue>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.InventoryOfficerId).IsRequired().HasMaxLength(100);
                entity.Property(x => x.InventoryOfficerName).IsRequired().HasMaxLength(150);
                entity.Property(x => x.UnitPrice).HasPrecision(18, 2);
                entity.Property(x => x.TotalAmount).HasPrecision(18, 2);
                entity.HasIndex(x => x.PartRequestId).IsUnique();
                entity.HasIndex(x => x.JobCardId);
                entity.HasOne(x => x.PartRequest).WithOne(x => x.PartIssue).HasForeignKey<PartIssue>(x => x.PartRequestId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(x => x.SparePart).WithMany().HasForeignKey(x => x.SparePartId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<ProcessedKafkaEvent>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.EventType).IsRequired().HasMaxLength(100);
                entity.HasIndex(x => x.EventId).IsUnique();
            });
        }
    }
}
