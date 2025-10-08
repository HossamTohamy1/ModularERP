using MediatR;
using ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_PriceRule;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Commends.Commends_PriceRule
{
    public class ReorderPriceListRulesCommand : IRequest<Unit>
    {
        public Guid PriceListId { get; set; }
        public ReorderPriceListRulesDTO Data { get; set; } = null!;
    }
}
