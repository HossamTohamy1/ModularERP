using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using ModularERP.Modules.Inventory.Features.Warehouses.Models;

namespace ModularERP.Modules.Inventory.Features.Warehouses.Configurations
{
    public class WarehouseStockConfiguration : IEntityTypeConfiguration<WarehouseStock>
    {
        public void Configure(EntityTypeBuilder<WarehouseStock> builder)
        {
            // Table Name
            builder.ToTable("WarehouseStocks", schema: "Inventory");

            // Primary Key
            builder.HasKey(ws => ws.Id);

            // Indexes
            builder.HasIndex(ws => new { ws.WarehouseId, ws.ProductId })
                .IsUnique()
                .HasDatabaseName("IX_WarehouseStock_Warehouse_Product");

            builder.HasIndex(ws => ws.ProductId)
                .HasDatabaseName("IX_WarehouseStock_Product");

            builder.HasIndex(ws => ws.WarehouseId)
                .HasDatabaseName("IX_WarehouseStock_Warehouse");

            // Relationships
            builder.HasOne(ws => ws.Warehouse)
                .WithMany()
                .HasForeignKey(ws => ws.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(ws => ws.Product)
                .WithMany()
                .HasForeignKey(ws => ws.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Computed Column (Optional - يحسب تلقائياً)
            builder.Property(ws => ws.AvailableQuantity)
                .HasComputedColumnSql("[Quantity] - ISNULL([ReservedQuantity], 0)", stored: true);

            // Default Values
            builder.Property(ws => ws.Quantity)
                .HasDefaultValue(0);

            builder.Property(ws => ws.ReservedQuantity)
                .HasDefaultValue(0m);

            builder.Property(ws => ws.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // Required Fields
            builder.Property(ws => ws.WarehouseId)
                .IsRequired();

            builder.Property(ws => ws.ProductId)
                .IsRequired();

            builder.Property(ws => ws.Quantity)
                .IsRequired();

            // Precision
            builder.Property(ws => ws.Quantity)
                .HasPrecision(18, 3);

            builder.Property(ws => ws.ReservedQuantity)
                .HasPrecision(18, 3);

            builder.Property(ws => ws.AvailableQuantity)
                .HasPrecision(18, 3);

            builder.Property(ws => ws.AverageUnitCost)
                .HasPrecision(18, 4);

            builder.Property(ws => ws.TotalValue)
                .HasPrecision(18, 4);

            builder.Property(ws => ws.MinStockLevel)
                .HasPrecision(18, 3);

            builder.Property(ws => ws.MaxStockLevel)
                .HasPrecision(18, 3);

            builder.Property(ws => ws.ReorderPoint)
                .HasPrecision(18, 3);
        }
    }
}