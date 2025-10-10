namespace ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_PriceCalculation
{
    public class PriceBreakdownDTO
    {
        public Guid TransactionId { get; set; }
        public string TransactionType { get; set; } = string.Empty;
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductSKU { get; set; } = string.Empty;
        public int Quantity { get; set; }

        public decimal BasePrice { get; set; }
        public decimal PriceAfterMarkup { get; set; }
        public decimal PriceAfterDiscount { get; set; }
        public decimal PriceBeforeTax { get; set; }
        public decimal FinalPrice { get; set; }

        public List<RuleApplicationDTO> AppliedRules { get; set; } = new();
        public List<DiscountApplicationDTO> AppliedDiscounts { get; set; } = new();
        public List<TaxApplicationDTO> AppliedTaxes { get; set; } = new();

        public decimal TotalDiscount { get; set; }
        public decimal TotalTax { get; set; }
        public decimal TotalAmount { get; set; }

        public DateTime CalculatedAt { get; set; }
        public string CalculatedBy { get; set; } = string.Empty;
    }
}
