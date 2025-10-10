using MediatR;
using ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_PriceCalculation;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Qeuries.Qeuires_PriceCalculation
{
    public class GetPriceBreakdownQuery : IRequest<PriceBreakdownDTO>
    {
        public Guid TransactionId { get; set; }
    }
}
