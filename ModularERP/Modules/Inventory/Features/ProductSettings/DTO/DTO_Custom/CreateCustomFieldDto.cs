using ModularERP.Common.Enum.Inventory_Enum;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.DTO.DTO_Custom
{
    public class CreateCustomFieldDto
    {
        public string FieldName { get; set; } = string.Empty;
        public string FieldLabel { get; set; } = string.Empty;
        public CustomFieldType FieldType { get; set; }
        public bool IsRequired { get; set; }
        public string? DefaultValue { get; set; }
        public string? Options { get; set; }
        public string? ValidationRules { get; set; }
        public int DisplayOrder { get; set; }
        public string? HelpText { get; set; }
    }
}
