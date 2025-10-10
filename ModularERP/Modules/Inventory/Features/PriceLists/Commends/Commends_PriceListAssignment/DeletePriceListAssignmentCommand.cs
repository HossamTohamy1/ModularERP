using MediatR;
using ModularERP.Common.ViewModel;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Commends.Commends_PriceListAssignment
{
    public class DeletePriceListAssignmentCommand : IRequest<ResponseViewModel<bool>>
    {
        public Guid Id { get; set; }
    }
}
