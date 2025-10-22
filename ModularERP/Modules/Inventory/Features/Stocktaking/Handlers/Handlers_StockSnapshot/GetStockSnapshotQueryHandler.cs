using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Stocktaking.DTO.DTO_StockSnapshot;
using ModularERP.Modules.Inventory.Features.Stocktaking.Models;
using ModularERP.Modules.Inventory.Features.Stocktaking.Qeuries.Qeuries_StockSnapshot;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Handlers.Handlers_StockSnapshot
{
    public class GetStockSnapshotQueryHandler : IRequestHandler<GetStockSnapshotQuery, ResponseViewModel<SnapshotListDto>>
    {
        private readonly IGeneralRepository<StocktakingHeader> _stocktakingRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetStockSnapshotQueryHandler> _logger;

        public GetStockSnapshotQueryHandler(
            IGeneralRepository<StocktakingHeader> stocktakingRepository,
            IMapper mapper,
            ILogger<GetStockSnapshotQueryHandler> logger)
        {
            _stocktakingRepository = stocktakingRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<SnapshotListDto>> Handle(GetStockSnapshotQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Fetching stock snapshot for stocktaking {StocktakingId}", request.StocktakingId);

                var stocktaking = await _stocktakingRepository
                    .GetAll()
                    .Where(s => s.Id == request.StocktakingId)
                    .Select(s => new
                    {
                        s.Id,
                        s.Number,
                        s.DateTime,
                        Snapshots = s.Snapshots.Select(snap => new
                        {
                            snap.SnapshotId,
                            snap.StocktakingId,
                            snap.ProductId,
                            snap.QtyAtStart,
                            snap.CreatedAt,
                            ProductName = snap.Product.Name,
                            ProductSKU = snap.Product.SKU
                        }).ToList()
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                if (stocktaking == null)
                {
                    _logger.LogWarning("Stocktaking {StocktakingId} not found", request.StocktakingId);
                    throw new NotFoundException(
                        $"Stocktaking with ID {request.StocktakingId} not found",
                        FinanceErrorCode.NotFound);
                }

                var result = new SnapshotListDto
                {
                    StocktakingId = stocktaking.Id,
                    StocktakingNumber = stocktaking.Number,
                    SnapshotDate = stocktaking.DateTime,
                    TotalProducts = stocktaking.Snapshots.Count,
                    Snapshots = stocktaking.Snapshots.Select(s => new StockSnapshotDto
                    {
                        SnapshotId = s.SnapshotId,
                        StocktakingId = s.StocktakingId,
                        ProductId = s.ProductId,
                        ProductName = s.ProductName,
                        ProductSKU = s.ProductSKU,
                        QtyAtStart = s.QtyAtStart,
                        CreatedAt = s.CreatedAt
                    }).ToList()
                };

                _logger.LogInformation("Successfully fetched {Count} snapshots for stocktaking {StocktakingId}",
                    result.TotalProducts, request.StocktakingId);

                return ResponseViewModel<SnapshotListDto>.Success(result, "Snapshot retrieved successfully");
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching stock snapshot for stocktaking {StocktakingId}", request.StocktakingId);
                throw new BusinessLogicException(
                    "Failed to fetch stock snapshot",
                    "Inventory",
                    FinanceErrorCode.InternalServerError);
            }
        }
    }
}
