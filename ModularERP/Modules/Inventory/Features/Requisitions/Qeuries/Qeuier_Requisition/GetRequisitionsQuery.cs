using MediatR;
using ModularERP.Common.Enum.Inventory_Enum;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Requisitions.DTO.DTO_Requisition;

namespace ModularERP.Modules.Inventory.Features.Requisitions.Handlers.Handlers_Requisition
{
    public class GetRequisitionsQuery : IRequest<ResponseViewModel<PaginatedResponseViewModel<RequisitionListDto>>>
    {
        public Guid CompanyId { get; set; }
        public Guid? WarehouseId { get; set; }
        public RequisitionStatus? Status { get; set; }
        public RequisitionType? Type { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string? SearchTerm { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}