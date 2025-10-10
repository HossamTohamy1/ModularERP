namespace ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_PriceCalculation
{
    public class PriceCalculationLogDTO
    {
        public Guid? TransactionId { get; set; }
        public string? TransactionType { get; set; }
        public string ProductName { get; set; }
        public string ProductSKU { get; set; }
        public Guid? ProductId { get; set; }
        public string? AppliedRule { get; set; }
        public decimal? ValueBefore { get; set; }
        public decimal? ValueAfter { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
