namespace ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_PriceCalculation
{
    public class PriceCalculationResultDTO
    {
        public Guid? TransactionId { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public decimal FinalPrice { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal TotalTax { get; set; }
        public List<PriceStepDTO> CalculationSteps { get; set; } = new();
        public DateTime CalculatedAt { get; set; }
    }
}
