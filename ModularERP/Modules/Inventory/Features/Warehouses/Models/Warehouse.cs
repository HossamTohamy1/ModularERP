using ModularERP.Common.Enum.Inventory_Enum;
using ModularERP.Common.Models;
using ModularERP.Modules.Finance.Features.Companys.Models;
using System.ComponentModel.DataAnnotations;

namespace ModularERP.Modules.Inventory.Features.Warehouses.Models
{
    public class Warehouse : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? ShippingAddress { get; set; }

        public WarehouseStatus Status { get; set; } = WarehouseStatus.Active;

        public bool IsPrimary { get; set; } = false;

        // Navigation properties
        public Guid CompanyId { get; set; }
        public virtual Company? Company { get; set; }
    }
}
