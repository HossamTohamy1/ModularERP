using ModularERP.Common.Models;
using ModularERP.Modules.Finance.Features.Companys.Models;
using System.ComponentModel.DataAnnotations;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.Models
{
    public class UnitConversion : BaseEntity
    {
        [Required]
        public Guid UnitTemplateId { get; set; }
        public virtual UnitTemplate? UnitTemplate { get; set; }

        [Required]
        [MaxLength(50)]
        public string UnitName { get; set; } = string.Empty; // e.g., "Kilogram"

        [Required]
        [MaxLength(10)]
        public string ShortName { get; set; } = string.Empty; // e.g., "kg"

        [Required]
        public decimal Factor { get; set; } // e.g., 1000 (1 kg = 1000 gm)

        public int DisplayOrder { get; set; } = 0;
        public Guid CompanyId { get; set; }
        public Company Company { get; set; }
    }

}
