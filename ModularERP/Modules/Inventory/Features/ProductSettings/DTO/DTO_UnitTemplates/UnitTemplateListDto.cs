using ModularERP.Common.Enum.Inventory_Enum;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.DTO.DTO_UnitTemplates
{
    public class UnitTemplateListDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string BaseUnitName { get; set; } = string.Empty;
        public string BaseUnitShortName { get; set; } = string.Empty;
        public UnitTemplateStatus Status { get; set; }
        public int ConversionsCount { get; set; }
    }
}
