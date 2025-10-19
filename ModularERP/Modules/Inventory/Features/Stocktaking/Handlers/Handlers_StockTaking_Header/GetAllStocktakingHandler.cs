using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Inventory_Enum;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Stocktaking.DTO.DTO_StockTaking_Header;
using ModularERP.Modules.Inventory.Features.Stocktaking.Models;
using ModularERP.Modules.Inventory.Features.Stocktaking.Qeuries.Qeuries_StockTaking_Header;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Handlers.Handlers_StockTaking_Header
{
    public class GetAllStocktakingHandler : IRequestHandler<GetAllStocktakingQuery, ResponseViewModel<List<StocktakingListDto>>>
    {
        private readonly IGeneralRepository<StocktakingHeader> _repo;
        private readonly ILogger<GetAllStocktakingHandler> _logger;

        public GetAllStocktakingHandler(
            IGeneralRepository<StocktakingHeader> repo,
            ILogger<GetAllStocktakingHandler> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<ResponseViewModel<List<StocktakingListDto>>> Handle(
            GetAllStocktakingQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Fetching all stocktakings");

                var query = _repo.GetAll();

                // Apply filters
                if (request.CompanyId.HasValue)
                {
                    query = query.Where(s => s.CompanyId == request.CompanyId.Value);
                }

                if (request.WarehouseId.HasValue)
                {
                    query = query.Where(s => s.WarehouseId == request.WarehouseId.Value);
                }

                if (!string.IsNullOrEmpty(request.Status) &&
                    Enum.TryParse<StocktakingStatus>(request.Status, true, out var status))
                {
                    query = query.Where(s => s.Status == status);
                }

                // Projection without Include
                var stocktakings = await query
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

                _logger.LogInformation("Fetched {Count} stocktakings", stocktakings.Count);

                return ResponseViewModel<List<StocktakingListDto>>.Success(
                    stocktakings,
                    $"Retrieved {stocktakings.Count} stocktaking(s) successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching stocktakings");
                throw new Exception("An error occurred while fetching stocktakings", ex);
            }
        }
    }
}