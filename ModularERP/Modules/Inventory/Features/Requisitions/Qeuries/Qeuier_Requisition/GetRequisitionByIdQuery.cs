using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Requisitions.DTO.DTO_Requisition;

namespace ModularERP.Modules.Inventory.Features.Requisitions.Qeuries.Qeuier_Requisition
{
    public class GetRequisitionByIdQuery : IRequest<ResponseViewModel<RequisitionResponseDto>>
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
    }
}
