using MediatR;
using ModularERP.Common.Enum.Inventory_Enum;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_PriceListAssignment;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Qeuries.Qeuires_PriceListAssignment
{
    public class GetPriceListAssignmentsByEntityQuery : IRequest<ResponseViewModel<List<PriceListAssignmentDto>>>
    {
        public PriceListEntityType EntityType { get; set; }
        public Guid EntityId { get; set; }
    }
}
