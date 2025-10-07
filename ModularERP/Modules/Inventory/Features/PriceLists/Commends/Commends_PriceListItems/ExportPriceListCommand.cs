using MediatR;
using ModularERP.Common.ViewModel;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Commends.Commends_PriceListItems
{
    public class ExportPriceListCommand : IRequest<ResponseViewModel<byte[]>>
    {
        public Guid Id { get; set; }

        public ExportPriceListCommand(Guid id)
        {
            Id = id;
        }
    }
}
