using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using ModularERP.Common.Models;

namespace ModularERP.Modules.Inventory.Features.Warehouses.Models
{
    public class WarehouseStock : BaseEntity
    {


        [Required]
        public Guid WarehouseId { get; set; }

        [Required]
        public Guid ProductId { get; set; }

        /// <summary>
        /// الكمية المتاحة حالياً
        /// </summary>
        [Column(TypeName = "decimal(18,3)")]
        public decimal Quantity { get; set; }

        /// <summary>
        /// الكمية المحجوزة (Reserved) - للطلبات قيد التنفيذ
        /// </summary>
        [Column(TypeName = "decimal(18,3)")]
        public decimal? ReservedQuantity { get; set; }

        /// <summary>
        /// الكمية المتاحة للبيع = Quantity - ReservedQuantity
        /// </summary>
        [Column(TypeName = "decimal(18,3)")]
        public decimal AvailableQuantity { get; set; }

        /// <summary>
        /// متوسط سعر التكلفة (Weighted Average Cost)
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal? AverageUnitCost { get; set; }

        /// <summary>
        /// إجمالي قيمة المخزون = Quantity × AverageUnitCost
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal? TotalValue { get; set; }

        /// <summary>
        /// الحد الأدنى للتنبيه (Low Stock Threshold)
        /// </summary>
        [Column(TypeName = "decimal(18,3)")]
        public decimal? MinStockLevel { get; set; }

        /// <summary>
        /// الحد الأقصى المسموح (Max Stock Level)
        /// </summary>
        [Column(TypeName = "decimal(18,3)")]
        public decimal? MaxStockLevel { get; set; }

        /// <summary>
        /// نقطة إعادة الطلب (Reorder Point)
        /// </summary>
        [Column(TypeName = "decimal(18,3)")]
        public decimal? ReorderPoint { get; set; }

        /// <summary>
        /// آخر تاريخ دخول للمخزن
        /// </summary>
        public DateTime? LastStockInDate { get; set; }

        /// <summary>
        /// آخر تاريخ خروج من المخزن
        /// </summary>
        public DateTime? LastStockOutDate { get; set; }




        // Navigation Properties
        public virtual Warehouse Warehouse { get; set; } = null!;
        public virtual Product Product { get; set; } = null!;
    }
}
