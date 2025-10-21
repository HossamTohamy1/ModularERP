using AutoMapper;
using MediatR;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Enum.Inventory_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Stocktaking.Commends.Commands_StockTraking_WorkFlow;
using ModularERP.Modules.Inventory.Features.Stocktaking.DTO.DTO_StockTaking_WorkFlow;
using ModularERP.Modules.Inventory.Features.Stocktaking.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Handlers.Handlers_StockTraking_WorkFlow
{
    public class ReviewStocktakingHandler : IRequestHandler<ReviewStocktakingCommand, ResponseViewModel<ReviewStocktakingDto>>
    {
        private readonly IGeneralRepository<StocktakingHeader> _stocktakingRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<ReviewStocktakingHandler> _logger;

        public ReviewStocktakingHandler(
            IGeneralRepository<StocktakingHeader> stocktakingRepo,
            IMapper mapper,
            ILogger<ReviewStocktakingHandler> logger)
        {
            _stocktakingRepo = stocktakingRepo;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<ReviewStocktakingDto>> Handle(
            ReviewStocktakingCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Moving stocktaking {StocktakingId} to review", request.StocktakingId);

                var stocktaking = await _stocktakingRepo.GetByID(request.StocktakingId);
                if (stocktaking == null)
                    throw new NotFoundException("Stocktaking not found", FinanceErrorCode.NotFound);

                if (stocktaking.Status != StocktakingStatus.Counting)
                    throw new BusinessLogicException("Only counting sessions can be moved to review", "Inventory");

                if (!stocktaking.Lines.Any())
                    throw new BusinessLogicException("Stocktaking must have at least one recorded item", "Inventory");

                stocktaking.Status = StocktakingStatus.Review;
                stocktaking.UpdatedAt = DateTime.UtcNow;
                await _stocktakingRepo.Update(stocktaking);

                var result = _mapper.Map<ReviewStocktakingDto>(stocktaking);
                return ResponseViewModel<ReviewStocktakingDto>.Success(result, "Stocktaking moved to review");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reviewing stocktaking {StocktakingId}", request.StocktakingId);
                throw;
            }
        }
    }
}
