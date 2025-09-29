using ModularERP.Common.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using ModularERP.Common.Enum.Inventory_Enum;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Models
{
    public class PriceListRule : BaseEntity
    {
        public Guid PriceListId { get; set; }

        [Required]
        public PriceRuleType RuleType { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal? Value { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public int Priority { get; set; } = 1;

        // Navigation Properties
        public virtual PriceList PriceList { get; set; } = null!;
    }
}
