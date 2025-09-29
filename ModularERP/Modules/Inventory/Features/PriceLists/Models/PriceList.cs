using ModularERP.Common.Enum.Inventory_Enum;
using ModularERP.Common.Models;
using ModularERP.Modules.Finance.Features.Currencies.Models;
using System.ComponentModel.DataAnnotations;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Models
{
    public class PriceList : BaseEntity
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public PriceListType Type { get; set; }

        [Required]
        [MaxLength(3)]
        public string CurrencyCode { get; set; } = string.Empty;

        public DateTime? ValidFrom { get; set; }

        public DateTime? ValidTo { get; set; }

        public bool IsDefault { get; set; } = false;

        public PriceListStatus Status { get; set; } = PriceListStatus.Active;

        public Guid TenantId { get; set; }
        public bool IsDeleted { get; set; }

        // Navigation Properties
        public virtual Currency Currency { get; set; } = null!;
        public virtual ICollection<PriceListItem> Items { get; set; } = new List<PriceListItem>();
        public virtual ICollection<PriceListRule> Rules { get; set; } = new List<PriceListRule>();
        public virtual ICollection<BulkDiscount> BulkDiscounts { get; set; } = new List<BulkDiscount>();
        public virtual ICollection<PriceListAssignment> Assignments { get; set; } = new List<PriceListAssignment>();
    }
}
