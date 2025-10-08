using MediatR;
using ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_PriceRule;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Qeuries.Qeuires_PriceRule
{
    public class GetPriceListRulesQuery : IRequest<List<PriceListRuleResponseDTO>>
    {
        public Guid PriceListId { get; set; }
    }
}
