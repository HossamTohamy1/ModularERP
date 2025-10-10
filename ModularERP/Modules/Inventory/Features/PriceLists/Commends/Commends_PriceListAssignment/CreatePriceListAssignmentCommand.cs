using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_PriceListAssignment;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Commends.Commends_PriceListAssignment
{
    public class CreatePriceListAssignmentCommand : IRequest<ResponseViewModel<PriceListAssignmentDto>>
    {
        public CreatePriceListAssignmentDto Data { get; set; } = null!;
    }
}
