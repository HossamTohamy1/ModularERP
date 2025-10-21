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
    public class StartStocktakingHandler : IRequestHandler<StartStocktakingCommand, ResponseViewModel<StartStocktakingDto>>
    {
        private readonly IGeneralRepository<StocktakingHeader> _stocktakingRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<StartStocktakingHandler> _logger;

        public StartStocktakingHandler(
            IGeneralRepository<StocktakingHeader> stocktakingRepo,
            IMapper mapper,
            ILogger<StartStocktakingHandler> logger)
        {
            _stocktakingRepo = stocktakingRepo;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<StartStocktakingDto>> Handle(
            StartStocktakingCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Starting stocktaking session {StocktakingId}", request.StocktakingId);

                var stocktaking = await _stocktakingRepo.GetByID(request.StocktakingId);
                if (stocktaking == null)
                    throw new NotFoundException("Stocktaking not found", FinanceErrorCode.NotFound);

                if (stocktaking.CompanyId != request.CompanyId)
                    throw new BusinessLogicException("Unauthorized access to stocktaking", "Inventory", FinanceErrorCode.UnauthorizedAccess);

                if (stocktaking.Status != StocktakingStatus.Draft)
                    throw new BusinessLogicException($"Cannot start stocktaking with status {stocktaking.Status}", "Inventory");

                stocktaking.Status = StocktakingStatus.Counting;
                await _stocktakingRepo.Update(stocktaking);

                var result = _mapper.Map<StartStocktakingDto>(stocktaking);
                return ResponseViewModel<StartStocktakingDto>.Success(result, "Stocktaking session started");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting stocktaking {StocktakingId}", request.StocktakingId);
                throw;
            }
        }
    }
}
