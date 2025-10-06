namespace ModularERP.Modules.Inventory.Features.ProductSettings.DTO.DTO_UnitTemplates
{
    public class CreateUnitConversionDto
    {
        public string UnitName { get; set; } = string.Empty;
        public string ShortName { get; set; } = string.Empty;
        public decimal Factor { get; set; }
        public int DisplayOrder { get; set; }
    }
}
