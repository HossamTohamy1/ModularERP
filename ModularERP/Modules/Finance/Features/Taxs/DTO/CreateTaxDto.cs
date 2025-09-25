using ModularERP.Common.Enum.Finance_Enum;
using System.ComponentModel.DataAnnotations;

namespace ModularERP.Modules.Finance.Features.Taxs.DTO
{
    public class CreateTaxDto
    {
        [Required(ErrorMessage = "Tax code is required")]
        [StringLength(20, ErrorMessage = "Tax code cannot exceed 20 characters")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tax name is required")]
        [StringLength(100, ErrorMessage = "Tax name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tax rate is required")]
        [Range(0, 100, ErrorMessage = "Tax rate must be between 0 and 100")]
        public decimal Rate { get; set; }

        [Required(ErrorMessage = "Tax type is required")]
        public TaxType Type { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
