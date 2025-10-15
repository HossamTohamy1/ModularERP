using MediatR;
using ModularERP.Common.ViewModel;
using System.ComponentModel.DataAnnotations;

namespace ModularERP.Modules.Inventory.Features.Requisitions.Commends.Commends_Requisition_Workflow
{
    public class ReverseRequisitionCommand : IRequest<ResponseViewModel<Guid>>
    {
        public Guid RequisitionId { get; set; }

        [Required(ErrorMessage = "Reversal reason is required")]
        [MinLength(10, ErrorMessage = "Reversal reason must be at least 10 characters")]
        public string Comments { get; set; } = string.Empty;
    }
}