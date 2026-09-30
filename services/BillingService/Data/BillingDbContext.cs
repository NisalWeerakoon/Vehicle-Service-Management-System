using Microsoft.EntityFrameworkCore;
using BillingService.Models;

namespace BillingService.Data
{
    public class BillingDbContext : DbContext
    {
        public BillingDbContext(
            DbContextOptions<BillingDbContext> options)
            : base(options)
        {
        }
        public DbSet<PartCharge> PartCharges => Set<PartCharge>();
        public DbSet<ProcessedKafkaEvent> ProcessedKafkaEvents => Set<ProcessedKafkaEvent>();
        public DbSet<Invoice> Invoices => Set<Invoice>();
        public DbSet<ChargeLine> ChargeLines => Set<ChargeLine>();
        public DbSet<Payment> Payments => Set<Payment>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PartCharge>(entity => { entity.HasKey(x => x.Id); entity.Property(x => x.SparePartName).IsRequired().HasMaxLength(200); entity.Property(x => x.UnitPrice).HasPrecision(18, 2); entity.Property(x => x.TotalAmount).HasPrecision(18, 2); entity.HasIndex(x => x.PartIssueId).IsUnique(); entity.HasIndex(x => x.JobCardId); });
            modelBuilder.Entity<ProcessedKafkaEvent>(entity => { entity.HasKey(x => x.Id); entity.Property(x => x.EventType).IsRequired().HasMaxLength(100); entity.HasIndex(x => x.EventId).IsUnique(); });
            modelBuilder.Entity<Invoice>(entity => { entity.HasKey(x => x.Id); entity.Property(x => x.InvoiceNumber).HasMaxLength(40); entity.Property(x => x.JobCardNumber).HasMaxLength(30); entity.Property(x => x.VehicleRegistrationNumber).HasMaxLength(30); entity.Property(x => x.TotalAmount).HasPrecision(18, 2); entity.HasIndex(x => x.JobCardId).IsUnique(); entity.HasIndex(x => x.InvoiceNumber).IsUnique(); entity.HasIndex(x => new { x.IsBillingEligible, x.IsGenerated }); });
            modelBuilder.Entity<ChargeLine>(entity => { entity.HasKey(x => x.Id); entity.Property(x => x.ChargeType).HasConversion<string>().HasMaxLength(20); entity.Property(x => x.Description).IsRequired().HasMaxLength(300); entity.Property(x => x.Quantity).HasPrecision(18, 2); entity.Property(x => x.UnitPrice).HasPrecision(18, 2); entity.Property(x => x.LineTotal).HasPrecision(18, 2); entity.Property(x => x.SourceReferenceId).HasMaxLength(100); entity.HasIndex(x => new { x.InvoiceId, x.SourceReferenceId }).IsUnique(); entity.HasOne(x => x.Invoice).WithMany(x => x.ChargeLines).HasForeignKey(x => x.InvoiceId).OnDelete(DeleteBehavior.Cascade); });
            modelBuilder.Entity<Invoice>(entity => { entity.Property(x => x.AmountPaid).HasPrecision(18, 2); entity.Property(x => x.PaymentStatus).HasConversion<string>().HasMaxLength(20); });
            modelBuilder.Entity<Payment>(entity => { entity.HasKey(x => x.Id); entity.Property(x => x.Amount).HasPrecision(18, 2); entity.Property(x => x.ReferenceNumber).IsRequired().HasMaxLength(100); entity.Property(x => x.CreatedBy).IsRequired().HasMaxLength(100); entity.HasIndex(x => x.ReferenceNumber).IsUnique(); entity.HasIndex(x => x.InvoiceId); entity.HasOne(x => x.Invoice).WithMany(x => x.Payments).HasForeignKey(x => x.InvoiceId).OnDelete(DeleteBehavior.Restrict); });
        }
    }
}
