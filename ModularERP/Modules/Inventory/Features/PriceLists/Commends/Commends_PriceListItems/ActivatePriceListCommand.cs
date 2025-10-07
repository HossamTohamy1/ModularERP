using MediatR;
using ModularERP.Common.ViewModel;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Commends.Commends_PriceListItems
{
    public class ActivatePriceListCommand : IRequest<ResponseViewModel<bool>>
    {
        public Guid Id { get; set; }

        public ActivatePriceListCommand(Guid id)
        {
            Id = id;
        }
    }
}
