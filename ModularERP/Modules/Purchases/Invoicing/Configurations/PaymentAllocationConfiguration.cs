using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using ModularERP.Modules.Purchases.Invoicing.Models;

namespace ModularERP.Modules.Purchases.Invoicing.Configurations
{
    public class PaymentAllocationConfiguration : IEntityTypeConfiguration<PaymentAllocation>
    {
        public void Configure(EntityTypeBuilder<PaymentAllocation> builder)
        {
            // ========================================
            // Table Configuration
            // ========================================
            builder.ToTable("PaymentAllocations", schema: "Purchases");

            // ========================================
            // Primary Key
            // ========================================
            builder.HasKey(pa => pa.Id);

            // ========================================
            // Properties Configuration
            // ========================================

            // PaymentId
            builder.Property(pa => pa.PaymentId)
                .IsRequired();

            // InvoiceId
            builder.Property(pa => pa.InvoiceId)
                .IsRequired();

            // AllocatedAmount
            builder.Property(pa => pa.AllocatedAmount)
                .HasPrecision(18, 2)
                .IsRequired();

            // AllocationDate
            builder.Property(pa => pa.AllocationDate)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // Notes
            builder.Property(pa => pa.Notes)
                .HasMaxLength(500);

            // AllocatedBy
            builder.Property(pa => pa.AllocatedBy)
                .HasMaxLength(450);

            // IsVoided
            builder.Property(pa => pa.IsVoided)
                .IsRequired()
                .HasDefaultValue(false);

            // VoidedAt
            builder.Property(pa => pa.VoidedAt)
                .IsRequired(false);

            // VoidedBy
            builder.Property(pa => pa.VoidedBy)
                .IsRequired(false);

            // VoidReason
            builder.Property(pa => pa.VoidReason)
                .HasMaxLength(500);

            // ========================================
            // Relationships
            // ========================================

            // Relationship: PaymentAllocation -> SupplierPayment (Many-to-One)
            builder.HasOne(pa => pa.Payment)
                .WithMany(p => p.Allocations)
                .HasForeignKey(pa => pa.PaymentId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete

            // Relationship: PaymentAllocation -> PurchaseInvoice (Many-to-One)
            builder.HasOne(pa => pa.Invoice)
                .WithMany(i => i.PaymentAllocations)
                .HasForeignKey(pa => pa.InvoiceId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete

            // Relationship: PaymentAllocation -> ApplicationUser (VoidedBy)
            builder.HasOne(pa => pa.VoidedByUser)
                .WithMany()
                .HasForeignKey(pa => pa.VoidedBy)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            // ========================================
            // Indexes
            // ========================================

            // Index on PaymentId for faster queries
            builder.HasIndex(pa => pa.PaymentId)
                .HasDatabaseName("IX_PaymentAllocations_PaymentId");

            // Index on InvoiceId for faster queries
            builder.HasIndex(pa => pa.InvoiceId)
                .HasDatabaseName("IX_PaymentAllocations_InvoiceId");

            // Composite index for unique constraint (one allocation per payment-invoice pair)
            builder.HasIndex(pa => new { pa.PaymentId, pa.InvoiceId })
                .HasDatabaseName("IX_PaymentAllocations_Payment_Invoice")
                .IsUnique();

            // Index on AllocationDate for reporting
            builder.HasIndex(pa => pa.AllocationDate)
                .HasDatabaseName("IX_PaymentAllocations_AllocationDate");

            // Index on IsVoided for filtering active allocations
            builder.HasIndex(pa => pa.IsVoided)
                .HasDatabaseName("IX_PaymentAllocations_IsVoided");

            // ========================================
            // Query Filters
            // ========================================

            // Global query filter to exclude deleted records
            builder.HasQueryFilter(pa => !pa.IsDeleted);
        }
    }
}
