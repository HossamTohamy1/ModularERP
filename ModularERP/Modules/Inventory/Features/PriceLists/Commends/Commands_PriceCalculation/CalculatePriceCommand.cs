using MediatR;
using ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_PriceCalculation;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Commends.Commands_PriceCalculation
{
    public class CalculatePriceCommand : IRequest<PriceCalculationResultDTO>
    {
        public Guid ProductId { get; set; }
        public Guid? PriceListId { get; set; }
        public Guid? CustomerId { get; set; }
        public int Quantity { get; set; } = 1;
        public string TransactionType { get; set; } = "Sale";
        public bool CreateLog { get; set; } = true;
        public Guid? UserId { get; set; }
    }
}
