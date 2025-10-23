using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Enum.Inventory_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Stocktaking.Commends.Commends_StockSnapshot;
using ModularERP.Modules.Inventory.Features.Stocktaking.DTO.DTO_StockSnapshot;
using ModularERP.Modules.Inventory.Features.Stocktaking.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Handlers.Handlers_StockSnapshot
{
    public class CreateSnapshotCommandHandler : IRequestHandler<CreateSnapshotCommand, ResponseViewModel<SnapshotListDto>>
    {
        private readonly IGeneralRepository<StocktakingHeader> _stocktakingRepository;
        private readonly IGeneralRepository<StockSnapshot> _snapshotRepository;
        private readonly IGeneralRepository<Product> _productRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateSnapshotCommandHandler> _logger;

        public CreateSnapshotCommandHandler(
            IGeneralRepository<StocktakingHeader> stocktakingRepository,
            IGeneralRepository<StockSnapshot> snapshotRepository,
            IGeneralRepository<Product> productRepository,
            IMapper mapper,
            ILogger<CreateSnapshotCommandHandler> logger)
        {
            _stocktakingRepository = stocktakingRepository;
            _snapshotRepository = snapshotRepository;
            _productRepository = productRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<SnapshotListDto>> Handle(CreateSnapshotCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Creating snapshot for stocktaking {StocktakingId}", request.StocktakingId);

                var stocktaking = await _stocktakingRepository
                    .GetAll()
                    .Where(s => s.Id == request.StocktakingId)
                    .Select(s => new
                    {
                        s.Id,
                        s.Number,
                        s.DateTime,
                        s.Status,
                        s.WarehouseId
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                if (stocktaking == null)
                {
                    _logger.LogWarning("Stocktaking {StocktakingId} not found", request.StocktakingId);
                    throw new NotFoundException(
                        $"Stocktaking with ID {request.StocktakingId} not found",
                        FinanceErrorCode.NotFound);
                }

                // Validate status
                if (stocktaking.Status != StocktakingStatus.Draft)
                {
                    _logger.LogWarning("Cannot create snapshot for stocktaking {StocktakingId} with status {Status}",
                        request.StocktakingId, stocktaking.Status);
                    throw new BusinessLogicException(
                        $"Cannot create snapshot. Stocktaking is in {stocktaking.Status} status",
                        "Inventory",
                        FinanceErrorCode.BusinessLogicError);
                }

                // Check if snapshot already exists
                var existingSnapshots = await _snapshotRepository
                    .GetAll()
                    .Where(s => s.StocktakingId == request.StocktakingId)
                    .AnyAsync(cancellationToken);

                if (existingSnapshots)
                {
                    _logger.LogWarning("Snapshot already exists for stocktaking {StocktakingId}", request.StocktakingId);
                    throw new BusinessLogicException(
                        "Snapshot already exists for this stocktaking",
                        "Inventory",
                        FinanceErrorCode.BusinessLogicError);
                }

                // Get all products in warehouse with stock > 0
                var products = await _productRepository
                    .GetAll()
                    .Where(p => p.InitialStock > 0) // Replace with warehouse stock logic
                    .Select(p => new
                    {
                        p.Id,
                        p.Name,
                        p.SKU,
                        CurrentQty = p.InitialStock
                    })
                    .ToListAsync(cancellationToken);

                // Create snapshots
                var snapshots = products.Select(p => new StockSnapshot
                {
                    SnapshotId = Guid.NewGuid(),
                    StocktakingId = request.StocktakingId,
                    ProductId = p.Id,
                    QtyAtStart = (decimal)p.CurrentQty,
                    CreatedAt = DateTime.UtcNow
                }).ToList();

                await _snapshotRepository.AddRangeAsync(snapshots);
                await _snapshotRepository.SaveChanges();

                _logger.LogInformation("Successfully created {Count} snapshots for stocktaking {StocktakingId}",
                    snapshots.Count, request.StocktakingId);

                var result = new SnapshotListDto
                {
                    StocktakingId = stocktaking.Id,
                    StocktakingNumber = stocktaking.Number,
                    SnapshotDate = stocktaking.DateTime,
                    TotalProducts = snapshots.Count,
                    Snapshots = snapshots.Select(s => new StockSnapshotDto
                    {
                        SnapshotId = s.SnapshotId,
                        StocktakingId = s.StocktakingId,
                        ProductId = s.ProductId,
                        ProductName = products.First(p => p.Id == s.ProductId).Name,
                        ProductSKU = products.First(p => p.Id == s.ProductId).SKU,
                        QtyAtStart = s.QtyAtStart,
                        CreatedAt = s.CreatedAt
                    }).ToList()
                };

                return ResponseViewModel<SnapshotListDto>.Success(result,
                    $"Successfully created {snapshots.Count} snapshots");
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (BusinessLogicException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating snapshot for stocktaking {StocktakingId}", request.StocktakingId);
                throw new BusinessLogicException(
                    "Failed to create snapshot",
                    "Inventory",
                    FinanceErrorCode.InternalServerError);
            }
        }
    }
}
