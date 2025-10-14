using MediatR;
using ModularERP.Common.ViewModel;

namespace ModularERP.Modules.Inventory.Features.Requisitions.Commends.Commends_RequisitionItem
{
    public class DeleteRequisitionItemCommand : IRequest<ResponseViewModel<bool>>
    {
        public Guid RequisitionId { get; set; }
        public Guid ItemId { get; set; }
    }
}