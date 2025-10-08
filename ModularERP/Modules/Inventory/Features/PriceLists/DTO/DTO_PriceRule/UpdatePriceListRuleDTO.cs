using ModularERP.Common.Enum.Inventory_Enum;

namespace ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_PriceRule
{
    public class UpdatePriceListRuleDTO
    {
        public PriceRuleType RuleType { get; set; }
        public decimal? Value { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int Priority { get; set; }
    }
}
