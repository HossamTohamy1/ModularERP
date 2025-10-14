using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Requisitions.DTO.DTO_Requisition;

namespace ModularERP.Modules.Inventory.Features.Requisitions.Commends.Commends_RequisitionItem
{
    public class BulkCreateRequisitionItemsCommand : IRequest<ResponseViewModel<List<RequisitionItemDto>>>
    {
        public Guid RequisitionId { get; set; }
        public BulkCreateRequisitionItemDTO BulkItems { get; set; } = null!;
    }
}