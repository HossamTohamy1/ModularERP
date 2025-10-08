using ModularERP.Common.Enum.Inventory_Enum;

namespace ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_PriceRule
{
    public class PriceListRuleResponseDTO
    {
        public Guid Id { get; set; }
        public Guid PriceListId { get; set; }
        public PriceRuleType RuleType { get; set; }
        public string RuleTypeName { get; set; } = string.Empty;
        public decimal? Value { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int Priority { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
