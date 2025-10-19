using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Stocktaking.DTO.DTO_StockTaking_Header;
using ModularERP.Modules.Inventory.Features.Stocktaking.Models;
using ModularERP.Modules.Inventory.Features.Stocktaking.Qeuries.Qeuries_StockTaking_Header;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Handlers.Handlers_StockTaking_Header
{
    public class GetStocktakingByWarehouseHandler : IRequestHandler<GetStocktakingByWarehouseQuery, ResponseViewModel<List<StocktakingListDto>>>
    {
        private readonly IGeneralRepository<StocktakingHeader> _repo;
        private readonly ILogger<GetStocktakingByWarehouseHandler> _logger;

        public GetStocktakingByWarehouseHandler(
            IGeneralRepository<StocktakingHeader> repo,
            ILogger<GetStocktakingByWarehouseHandler> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<ResponseViewModel<List<StocktakingListDto>>> Handle(
            GetStocktakingByWarehouseQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Fetching stocktakings for warehouse {WarehouseId}", request.WarehouseId);

                var stocktakings = await _repo
                    .Get(s => s.WarehouseId == request.WarehouseId)
                    .Select(s => new StocktakingListDto
                    {
                        Id = s.Id,
                        Number = s.Number,
                        DateTime = s.DateTime,
                        WarehouseName = s.Warehouse != null ? s.Warehouse.Name : string.Empty,
                        CompanyName = s.Company != null ? s.Company.Name : string.Empty,
                        Status = s.Status,
                        UpdateSystem = s.UpdateSystem,
                        LineCount = s.Lines.Count,
                        ApprovedByName = s.ApprovedByUser != null ? s.ApprovedByUser.UserName : null,
                        ApprovedAt = s.ApprovedAt,
                        CreatedAt = s.CreatedAt
                    })
                    .OrderByDescending(s => s.CreatedAt)
                    .ToListAsync(cancellationToken);

                _logger.LogInformation("Fetched {Count} stocktakings for warehouse {WarehouseId}",
                    stocktakings.Count, request.WarehouseId);

                return ResponseViewModel<List<StocktakingListDto>>.Success(
                    stocktakings,
                    $"Retrieved {stocktakings.Count} stocktaking(s) successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching stocktakings for warehouse {WarehouseId}", request.WarehouseId);
                throw new Exception($"An error occurred while fetching stocktakings for warehouse {request.WarehouseId}", ex);
            }
        }
    }
}