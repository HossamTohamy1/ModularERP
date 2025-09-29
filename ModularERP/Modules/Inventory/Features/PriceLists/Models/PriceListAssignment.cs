using ModularERP.Common.Enum.Inventory_Enum;
using ModularERP.Common.Models;
using System.ComponentModel.DataAnnotations;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Models
{
    public class PriceListAssignment : BaseEntity
    {
        [Required]
        public PriceListEntityType EntityType { get; set; }

        /// <summary>
        /// ID of customer/supplier/group depending on EntityType
        /// </summary>
        public Guid EntityId { get; set; }

        public Guid PriceListId { get; set; }

        // Navigation Properties
        public virtual PriceList PriceList { get; set; } = null!;
    }
}
