using MediatR;
using ModularERP.Modules.Inventory.Features.PriceLists.Commends.Commands_PriceCalculation;
using ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_PriceCalculation;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Handlers.Handlers_PriceCalculation
{
    public class PreviewPriceCommandHandler : IRequestHandler<PreviewPriceCommand, PriceCalculationResultDTO>
    {
        private readonly IMediator _mediator;

        public PreviewPriceCommandHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<PriceCalculationResultDTO> Handle(PreviewPriceCommand request, CancellationToken ct)
        {
            var calculateCommand = new CalculatePriceCommand
            {
                ProductId = request.ProductId,
                PriceListId = request.PriceListId,
                CustomerId = request.CustomerId,
                Quantity = request.Quantity,
                TransactionType = request.TransactionType,
                CreateLog = false
            };

            return await _mediator.Send(calculateCommand, ct);
        }
    }
}