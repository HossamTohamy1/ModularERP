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
    public class CreateStocktakingLineHandler : IRequestHandler<CreateStocktakingLineCommand, ResponseViewModel<StocktakingLineDto>>
    {
        private readonly IGeneralRepository<StocktakingLine> _lineRepository;
        private readonly IGeneralRepository<StocktakingHeader> _headerRepository;
        private readonly IGeneralRepository<Product> _productRepository;
        private readonly IGeneralRepository<WarehouseStock> _stockRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateStocktakingLineHandler> _logger;

        public CreateStocktakingLineHandler(
            IGeneralRepository<StocktakingLine> lineRepository,
            IGeneralRepository<StocktakingHeader> headerRepository,
            IGeneralRepository<Product> productRepository,
            IGeneralRepository<WarehouseStock> stockRepository,
            IMapper mapper,
            ILogger<CreateStocktakingLineHandler> logger)
        {
            _lineRepository = lineRepository;
            _headerRepository = headerRepository;
            _productRepository = productRepository;
            _stockRepository = stockRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<StocktakingLineDto>> Handle(
            CreateStocktakingLineCommand request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Processing CreateStocktakingLineCommand for stocktaking {StocktakingId}",
                request.StocktakingId);

            // Validate stocktaking exists
            var stocktaking = await _headerRepository.GetByID(request.StocktakingId);
            if (stocktaking == null)
            {
                _logger.LogWarning("Stocktaking {StocktakingId} not found", request.StocktakingId);
                throw new NotFoundException(
                    $"Stocktaking with ID {request.StocktakingId} not found",
                    FinanceErrorCode.NotFound);
            }

            // Validate status - only Draft or Counting allowed
            if (stocktaking.Status != StocktakingStatus.Draft &&
                stocktaking.Status != StocktakingStatus.Counting)
            {
                _logger.LogWarning("Cannot add lines to stocktaking {StocktakingId} with status {Status}",
                    request.StocktakingId, stocktaking.Status);
                throw new BusinessLogicException(
                    $"Cannot add lines to stocktaking with status {stocktaking.Status}",
                    "Inventory",
                    FinanceErrorCode.BusinessLogicError);
            }

            // Validate product exists
            var productExists = await _productRepository
                .AnyAsync(p => p.Id == request.ProductId, cancellationToken);

            if (!productExists)
            {
                _logger.LogWarning("Product {ProductId} not found", request.ProductId);
                throw new NotFoundException(
                    $"Product with ID {request.ProductId} not found",
                    FinanceErrorCode.NotFound);
            }

            // Check for duplicate product in same stocktaking
            var isDuplicate = await _lineRepository
                .AnyAsync(l => l.StocktakingId == request.StocktakingId &&
                              l.ProductId == request.ProductId,
                         cancellationToken);

            if (isDuplicate)
            {
                _logger.LogWarning("Product {ProductId} already exists in stocktaking {StocktakingId}",
                    request.ProductId, request.StocktakingId);
                throw new BusinessLogicException(
                    "This product already exists in the stocktaking",
                    "Inventory",
                    FinanceErrorCode.DuplicateEntity);
            }

            // Validate physical quantity
            if (request.PhysicalQty < 0)
            {
                throw new BusinessLogicException(
                    "Physical quantity cannot be negative",
                    "Inventory",
                    FinanceErrorCode.ValidationError);
            }

            // Get current system quantity from warehouse stock
            var warehouseStock = await _stockRepository
                .Get(ws => ws.WarehouseId == stocktaking.WarehouseId &&
                          ws.ProductId == request.ProductId)
                .FirstOrDefaultAsync(cancellationToken);

            var systemQty = warehouseStock?.Quantity ?? 0;

            // Create line
            var line = new StocktakingLine
            {
                Id = Guid.NewGuid(),
                StocktakingId = request.StocktakingId,
                ProductId = request.ProductId,
                UnitId = request.UnitId,
                PhysicalQty = request.PhysicalQty,
                SystemQtySnapshot = systemQty,
                Note = request.Note,
                ImagePath = request.ImagePath,
                CreatedAt = DateTime.UtcNow
            };

            // Calculate variance
            line.VarianceQty = line.PhysicalQty - line.SystemQtySnapshot;
            line.ValuationCost = warehouseStock?.AverageUnitCost ?? 0;
            line.VarianceValue = line.VarianceQty * line.ValuationCost;

            await _lineRepository.AddAsync(line);
            await _lineRepository.SaveChanges();

            // Update stocktaking status to Counting if it was Draft
            if (stocktaking.Status == StocktakingStatus.Draft)
            {
                stocktaking.Status = StocktakingStatus.Counting;
                await _headerRepository.Update(stocktaking);
            }

            _logger.LogInformation("Stocktaking line created successfully with ID {LineId}", line.Id);

            // Get created line with projection
            var createdLine = await _lineRepository
                .Get(l => l.Id == line.Id)
                .ProjectTo<StocktakingLineDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);

            return ResponseViewModel<StocktakingLineDto>.Success(
                createdLine,
                "Stocktaking line created successfully");
        }
    }
}
