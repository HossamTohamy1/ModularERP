using ModularERP.Common.Enum.Inventory_Enum;
using ModularERP.Common.Models;
using ModularERP.Modules.Inventory.Features.Products.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Models
{
    public class BulkDiscount : BaseEntity
    {
        public Guid ProductId { get; set; }

        public Guid PriceListId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,3)")]
        public decimal MinQty { get; set; }

        [Column(TypeName = "decimal(18,3)")]
        public decimal? MaxQty { get; set; }

        [Required]
        public DiscountType DiscountType { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal DiscountValue { get; set; }

        // Navigation Properties
        public virtual Product Product { get; set; } = null!;
        public virtual PriceList PriceList { get; set; } = null!;
    }
}
