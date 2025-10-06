namespace ModularERP.Modules.Inventory.Features.ProductSettings.DTO.DTO_UnitTemplates
{
    public class CreateUnitTemplateDto
    {
        public string Name { get; set; } = string.Empty;
        public string BaseUnitName { get; set; } = string.Empty;
        public string BaseUnitShortName { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
