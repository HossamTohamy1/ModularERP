using MediatR;
using ModularERP.Common.ViewModel;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Commends.Commends_PriceListItems
{
    public class DeletePriceListCommand : IRequest<ResponseViewModel<bool>>
    {
        public Guid Id { get; set; }

        public DeletePriceListCommand(Guid id)
        {
            Id = id;
        }
    }
}
