using MediatR;
using ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_PriceCalculation;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Commends.Commands_PriceCalculation
{
    public class PreviewPriceCommand : IRequest<PriceCalculationResultDTO>
    {
        public Guid ProductId { get; set; }
        public Guid? PriceListId { get; set; }
        public Guid? CustomerId { get; set; }
        public int Quantity { get; set; } = 1;
        public string TransactionType { get; set; } = "Sale";
    }
}
