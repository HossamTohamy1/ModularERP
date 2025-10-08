using MediatR;
using ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_PriceRule;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Commends.Commends_PriceRule
{
    public class UpdatePriceListRuleCommand : IRequest<PriceListRuleResponseDTO>
    {
        public Guid PriceListId { get; set; }
        public Guid RuleId { get; set; }
        public UpdatePriceListRuleDTO Data { get; set; } = null!;
    }

}
