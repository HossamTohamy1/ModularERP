using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_PriceListAssignment;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Commends.Commends_PriceListAssignment
{
    public class UpdatePriceListAssignmentCommand : IRequest<ResponseViewModel<PriceListAssignmentDto>>
    {
        public Guid Id { get; set; }
        public UpdatePriceListAssignmentDto Data { get; set; } = null!;
    }
}
