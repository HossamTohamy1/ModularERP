namespace ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_PriceCalculation
{
    public class TaxApplicationDTO
    {
        public string TaxName { get; set; } = string.Empty;
        public decimal TaxRate { get; set; }
        public decimal TaxAmount { get; set; }
        public bool IsInclusive { get; set; }
    }
}
