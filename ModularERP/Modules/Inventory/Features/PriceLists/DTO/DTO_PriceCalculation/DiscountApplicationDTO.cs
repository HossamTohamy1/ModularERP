namespace ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_PriceCalculation
{
    public class DiscountApplicationDTO
    {
        public string DiscountType { get; set; } = string.Empty;
        public decimal DiscountValue { get; set; }
        public decimal DiscountAmount { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
