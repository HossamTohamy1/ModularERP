using MediatR;
using ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_PriceRule;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Commends.Commends_PriceRule
{
    public class CreatePriceListRuleCommand : IRequest<PriceListRuleResponseDTO>
    {
        public Guid PriceListId { get; set; }
        public CreatePriceListRuleDTO Data { get; set; } = null!;
    }
}
