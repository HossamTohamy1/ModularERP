using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Stocktaking.DTO.DTO_StockTaking_Header;
using ModularERP.Modules.Inventory.Features.Stocktaking.Models;
using ModularERP.Modules.Inventory.Features.Stocktaking.Qeuries.Qeuries_StockTaking_Header;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Handlers.Handlers_StockTaking_Header
{
    public class GetStocktakingByStatusHandler : IRequestHandler<GetStocktakingByStatusQuery, ResponseViewModel<List<StocktakingListDto>>>
    {
        private readonly IGeneralRepository<StocktakingHeader> _repo;
        private readonly ILogger<GetStocktakingByStatusHandler> _logger;

        public GetStocktakingByStatusHandler(
            IGeneralRepository<StocktakingHeader> repo,
            ILogger<GetStocktakingByStatusHandler> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<ResponseViewModel<List<StocktakingListDto>>> Handle(
            GetStocktakingByStatusQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Fetching stocktakings with status {Status}", request.Status);

                var stocktakings = await _repo
                    .Get(s => s.Status == request.Status)
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

                _logger.LogInformation("Fetched {Count} stocktakings with status {Status}",
                    stocktakings.Count, request.Status);

                return ResponseViewModel<List<StocktakingListDto>>.Success(
                    stocktakings,
                    $"Retrieved {stocktakings.Count} stocktaking(s) successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching stocktakings with status {Status}", request.Status);
                throw new Exception($"An error occurred while fetching stocktakings with status {request.Status}", ex);
            }
        }
    }
}