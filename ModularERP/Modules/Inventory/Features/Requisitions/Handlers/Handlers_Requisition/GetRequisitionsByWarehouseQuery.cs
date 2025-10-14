using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Requisitions.DTO.DTO_Requisition;

namespace ModularERP.Modules.Inventory.Features.Requisitions.Handlers.Handlers_Requisition
{
    public class GetRequisitionsByWarehouseQuery : IRequest<ResponseViewModel<List<RequisitionListDto>>>
    {
        public Guid CompanyId { get; set; }
        public Guid WarehouseId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}