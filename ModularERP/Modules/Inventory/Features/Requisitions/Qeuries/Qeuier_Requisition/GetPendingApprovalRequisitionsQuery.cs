using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Requisitions.DTO.DTO_Requisition;

namespace ModularERP.Modules.Inventory.Features.Requisitions.Qeuries.Qeuier_Requisition
{
    public class GetPendingApprovalRequisitionsQuery : IRequest<ResponseViewModel<List<RequisitionListDto>>>
    {
        public Guid CompanyId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}