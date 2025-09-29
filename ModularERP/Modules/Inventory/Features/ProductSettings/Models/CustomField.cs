using ModularERP.Common.Enum.Inventory_Enum;
using ModularERP.Common.Models;
using System.ComponentModel.DataAnnotations;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.Models
{
    public class CustomField : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string FieldName { get; set; } = string.Empty; // e.g., "Warranty Period"

        [Required]
        [MaxLength(100)]
        public string FieldLabel { get; set; } = string.Empty; // Display label

        [Required]
        public CustomFieldType FieldType { get; set; } = CustomFieldType.Text;

        public bool IsRequired { get; set; } = false;

        [MaxLength(1000)]
        public string? DefaultValue { get; set; }

        [MaxLength(2000)]
        public string? Options { get; set; } // JSON array for dropdown options

        [MaxLength(500)]
        public string? ValidationRules { get; set; } // JSON for validation rules

        public int DisplayOrder { get; set; } = 0;

        public CustomFieldStatus Status { get; set; } = CustomFieldStatus.Active;

        [MaxLength(500)]
        public string? HelpText { get; set; }
    }
}
