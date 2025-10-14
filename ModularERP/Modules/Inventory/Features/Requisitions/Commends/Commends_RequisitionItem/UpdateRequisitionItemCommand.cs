using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Requisitions.DTO.DTO_Requisition;

namespace ModularERP.Modules.Inventory.Features.Requisitions.Commends.Commends_RequisitionItem
{
    public class UpdateRequisitionItemCommand : IRequest<ResponseViewModel<RequisitionItemDto>>
    {
        public Guid RequisitionId { get; set; }
        public Guid ItemId { get; set; }
        public UpdateRequisitionItemDTO Item { get; set; } = null!;
    }
}