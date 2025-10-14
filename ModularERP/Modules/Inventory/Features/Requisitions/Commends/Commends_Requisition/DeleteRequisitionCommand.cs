using MediatR;
using ModularERP.Common.ViewModel;

namespace ModularERP.Modules.Inventory.Features.Requisitions.Commends.Commends_Requisition
{
    public class DeleteRequisitionCommand : IRequest<ResponseViewModel<bool>>
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
    }
}
