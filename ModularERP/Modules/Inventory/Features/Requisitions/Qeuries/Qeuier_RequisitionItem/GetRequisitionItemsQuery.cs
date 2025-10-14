using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Requisitions.DTO.DTO_Requisition;

namespace ModularERP.Modules.Inventory.Features.Requisitions.Qeuries.Qeuier_RequisitionItem
{
    public class GetRequisitionItemsQuery : IRequest<ResponseViewModel<List<RequisitionItemDto>>>
    {
        public Guid RequisitionId { get; set; }
    }
}
