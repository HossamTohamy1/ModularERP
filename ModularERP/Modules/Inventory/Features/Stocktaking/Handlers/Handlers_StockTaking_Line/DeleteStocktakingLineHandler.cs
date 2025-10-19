using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Enum.Inventory_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Stocktaking.Commends.Commends_StockTaking_Line;
using ModularERP.Modules.Inventory.Features.Stocktaking.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Handlers.Handlers_StockTaking_Line
{
    public class DeleteStocktakingLineHandler : IRequestHandler<DeleteStocktakingLineCommand, ResponseViewModel<bool>>
    {
        private readonly IGeneralRepository<StocktakingLine> _lineRepository;
        private readonly IGeneralRepository<StocktakingHeader> _headerRepository;
        private readonly ILogger<DeleteStocktakingLineHandler> _logger;

        public DeleteStocktakingLineHandler(
            IGeneralRepository<StocktakingLine> lineRepository,
            IGeneralRepository<StocktakingHeader> headerRepository,
            ILogger<DeleteStocktakingLineHandler> logger)
        {
            _lineRepository = lineRepository;
            _headerRepository = headerRepository;
            _logger = logger;
        }

        public async Task<ResponseViewModel<bool>> Handle(
            DeleteStocktakingLineCommand request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Processing DeleteStocktakingLineCommand for line {LineId}",
                request.LineId);

            // Get line
            var line = await _lineRepository
                .Get(l => l.Id == request.LineId &&
                         l.StocktakingId == request.StocktakingId)
                .FirstOrDefaultAsync(cancellationToken);

            if (line == null)
            {
                _logger.LogWarning("Stocktaking line {LineId} not found", request.LineId);
                throw new NotFoundException(
                    $"Stocktaking line with ID {request.LineId} not found",
                    FinanceErrorCode.NotFound);
            }

            // Validate stocktaking status
            var stocktaking = await _headerRepository.GetByID(request.StocktakingId);
            if (stocktaking.Status != StocktakingStatus.Draft &&
                stocktaking.Status != StocktakingStatus.Counting)
            {
                _logger.LogWarning("Cannot delete line from stocktaking {StocktakingId} with status {Status}",
                    request.StocktakingId, stocktaking.Status);
                throw new BusinessLogicException(
                    $"Cannot delete lines from stocktaking with status {stocktaking.Status}",
                    "Inventory",
                    FinanceErrorCode.BusinessLogicError);
            }

            // Soft delete
            await _lineRepository.Delete(request.LineId);

            _logger.LogInformation("Stocktaking line {LineId} deleted successfully", request.LineId);

            // Check if stocktaking has no more lines, revert to Draft
            var remainingLinesCount = await _lineRepository
                .Get(l => l.StocktakingId == request.StocktakingId)
                .CountAsync(cancellationToken);

            if (remainingLinesCount == 0 && stocktaking.Status == StocktakingStatus.Counting)
            {
                stocktaking.Status = StocktakingStatus.Draft;
                await _headerRepository.Update(stocktaking);
                _logger.LogInformation("Stocktaking {StocktakingId} reverted to Draft status",
                    request.StocktakingId);
            }

            return ResponseViewModel<bool>.Success(
                true,
                "Stocktaking line deleted successfully");
        }
    }
}