using MediatR;
using ModularERP.Common.ViewModel;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Commends.Commends_PriceListItems
{
    public class DeactivatePriceListCommand : IRequest<ResponseViewModel<bool>>
    {
        public Guid Id { get; set; }

        public DeactivatePriceListCommand(Guid id)
        {
            Id = id;
        }
    }

}
