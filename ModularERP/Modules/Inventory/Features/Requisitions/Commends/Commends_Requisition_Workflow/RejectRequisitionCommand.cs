using MediatR;
using ModularERP.Common.ViewModel;
using System.ComponentModel.DataAnnotations;

namespace ModularERP.Modules.Inventory.Features.Requisitions.Commends.Commends_Requisition_Workflow
{
    public class RejectRequisitionCommand : IRequest<ResponseViewModel<bool>>
    {
        public Guid RequisitionId { get; set; }

        [Required(ErrorMessage = "Rejection reason is required")]
        [MinLength(10, ErrorMessage = "Rejection reason must be at least 10 characters")]
        public string Comments { get; set; } = string.Empty;
    }
}