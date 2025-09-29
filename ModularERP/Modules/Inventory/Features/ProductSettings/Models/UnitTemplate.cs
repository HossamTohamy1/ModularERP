using ModularERP.Common.Enum.Inventory_Enum;
using ModularERP.Common.Models;
using System.ComponentModel.DataAnnotations;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.Models
{
    public class UnitTemplate : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty; // e.g., "Weight", "Volume"

        [Required]
        [MaxLength(50)]
        public string BaseUnitName { get; set; } = string.Empty; // e.g., "Gram", "Liter"

        [Required]
        [MaxLength(10)]
        public string BaseUnitShortName { get; set; } = string.Empty; // e.g., "gm", "L"

        [MaxLength(500)]
        public string? Description { get; set; }

        public UnitTemplateStatus Status { get; set; } = UnitTemplateStatus.Active;

        // Navigation properties
        public virtual ICollection<UnitConversion> UnitConversions { get; set; } = new List<UnitConversion>();
    }
}
