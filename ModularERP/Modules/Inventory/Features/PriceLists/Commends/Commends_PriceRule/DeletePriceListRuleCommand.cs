using MediatR;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Commends.Commends_PriceRule
{
    public class DeletePriceListRuleCommand : IRequest<Unit>
    {
        public Guid PriceListId { get; set; }
        public Guid RuleId { get; set; }
    }
}
