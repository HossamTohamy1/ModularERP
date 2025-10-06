using ModularERP.Common.Enum.Inventory_Enum;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.DTO.DTO_UnitTemplates
{
    public class UpdateUnitTemplateDto
    {
        public string Name { get; set; } = string.Empty;
        public string BaseUnitName { get; set; } = string.Empty;
        public string BaseUnitShortName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public UnitTemplateStatus Status { get; set; }
    }
}
