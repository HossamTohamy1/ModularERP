using ModularERP.Common.Models;
using ModularERP.Modules.Inventory.Features.Products.Models;
using ModularERP.Modules.Inventory.Features.Services.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using ModularERP.Modules.Inventory.Features.TaxManagement.Models;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Models
{
    public class PriceListItem : BaseEntity
    {
        public Guid PriceListId { get; set; }

        public Guid? ProductId { get; set; }

        public Guid? ServiceId { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal? BasePrice { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal? ListPrice { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal? DiscountValue { get; set; }

        [MaxLength(20)]
        public string? DiscountType { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal? FinalPrice { get; set; }

        public Guid? TaxProfileId { get; set; }

        public DateTime? ValidFrom { get; set; }

        public DateTime? ValidTo { get; set; }

        // Navigation Properties
        public virtual PriceList PriceList { get; set; } = null!;
        public virtual Product? Product { get; set; }
        public virtual Service? Service { get; set; }
        public virtual TaxProfile? TaxProfile { get; set; }
    }
}
