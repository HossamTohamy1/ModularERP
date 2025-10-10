namespace ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_PriceCalculation
{
    public class PriceStepDTO
    {
        public int StepOrder { get; set; }
        public string RuleName { get; set; } = string.Empty;
        public string RuleType { get; set; } = string.Empty;
        public decimal ValueBefore { get; set; }
        public decimal ValueAfter { get; set; }
        public decimal AdjustmentAmount { get; set; }
    }
}
