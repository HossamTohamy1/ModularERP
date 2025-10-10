using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_PriceListAssignment;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Qeuries.Qeuires_PriceListAssignment
{
    public class GetAllPriceListAssignmentsQuery : IRequest<ResponseViewModel<List<PriceListAssignmentDto>>>
    {
    }
}
