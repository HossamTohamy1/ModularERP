using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Enum.Inventory_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Stocktaking.Commends.Commends_StockTaking_Line;
using ModularERP.Modules.Inventory.Features.Stocktaking.DTO.DTO_StockTaking_Line;
using ModularERP.Modules.Inventory.Features.Stocktaking.Models;
using ModularERP.Modules.Inventory.Features.Warehouses.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Handlers.Handlers_StockTaking_Line
{
    public class UpdateStocktakingLineHandler : IRequestHandler<UpdateStocktakingLineCommand, ResponseViewModel<StocktakingLineDto>>
    {
        private readonly IGeneralRepository<StocktakingLine> _lineRepository;
        private readonly IGeneralRepository<StocktakingHeader> _headerRepository;
        private readonly IGeneralRepository<WarehouseStock> _stockRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<UpdateStocktakingLineHandler> _logger;

        public UpdateStocktakingLineHandler(
            IGeneralRepository<StocktakingLine> lineRepository,
            IGeneralRepository<StocktakingHeader> headerRepository,
            IGeneralRepository<WarehouseStock> stockRepository,
            IMapper mapper,
            ILogger<UpdateStocktakingLineHandler> logger)
        {
            _lineRepository = lineRepository;
            _headerRepository = headerRepository;
            _stockRepository = stockRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<StocktakingLineDto>> Handle(
            UpdateStocktakingLineCommand request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Processing UpdateStocktakingLineCommand for line {LineId}",
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
                _logger.LogWarning("Cannot update line in stocktaking {StocktakingId} with status {Status}",
                    request.StocktakingId, stocktaking.Status);
                throw new BusinessLogicException(
                    $"Cannot update lines in stocktaking with status {stocktaking.Status}",
                    "Inventory",
                    FinanceErrorCode.BusinessLogicError);
            }

            // Validate physical quantity
            if (request.PhysicalQty < 0)
            {
                throw new BusinessLogicException(
                    "Physical quantity cannot be negative",
                    "Inventory",
                    FinanceErrorCode.ValidationError);
            }

            // Update line
            line.PhysicalQty = request.PhysicalQty;
            line.Note = request.Note ?? line.Note;
            line.ImagePath = request.ImagePath ?? line.ImagePath;
            line.UpdatedAt = DateTime.UtcNow;

            // Recalculate variance
            line.VarianceQty = line.PhysicalQty - line.SystemQtySnapshot;

            // Get current warehouse stock for valuation
            var warehouseStock = await _stockRepository
                .Get(ws => ws.WarehouseId == stocktaking.WarehouseId &&
                          ws.ProductId == line.ProductId)
                .FirstOrDefaultAsync(cancellationToken);

            line.ValuationCost = warehouseStock?.AverageUnitCost ?? line.ValuationCost ?? 0;
            line.VarianceValue = line.VarianceQty * line.ValuationCost;

            await _lineRepository.Update(line);

            _logger.LogInformation("Stocktaking line {LineId} updated successfully", request.LineId);

            // Get updated line with projection
            var updatedLine = await _lineRepository
                .Get(l => l.Id == request.LineId)
                .ProjectTo<StocktakingLineDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);

            return ResponseViewModel<StocktakingLineDto>.Success(
                updatedLine,
                "Stocktaking line updated successfully");
        }
    }
}
