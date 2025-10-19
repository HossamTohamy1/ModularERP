using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Stocktaking.DTO.DTO_StockTaking_Header;
using ModularERP.Modules.Inventory.Features.Stocktaking.Models;
using ModularERP.Modules.Inventory.Features.Stocktaking.Qeuries.Qeuries_StockTaking_Header;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Handlers.Handlers_StockTaking_Header
{
    public class GetStocktakingByIdHandler : IRequestHandler<GetStocktakingByIdQuery, ResponseViewModel<StocktakingDetailDto>>
    {
        private readonly IGeneralRepository<StocktakingHeader> _repo;
        private readonly ILogger<GetStocktakingByIdHandler> _logger;

        public GetStocktakingByIdHandler(
            IGeneralRepository<StocktakingHeader> repo,
            ILogger<GetStocktakingByIdHandler> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<ResponseViewModel<StocktakingDetailDto>> Handle(
            GetStocktakingByIdQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Fetching stocktaking with ID {StocktakingId}", request.Id);

                // Projection without Include
                var stocktaking = await _repo
                    .Get(s => s.Id == request.Id)
                    .Select(s => new StocktakingDetailDto
                    {
                        Id = s.Id,
                        WarehouseId = s.WarehouseId,
                        WarehouseName = s.Warehouse != null ? s.Warehouse.Name : string.Empty,
                        CompanyId = s.CompanyId,
                        CompanyName = s.Company != null ? s.Company.Name : string.Empty,
                        Number = s.Number,
                        DateTime = s.DateTime,
                        Notes = s.Notes,
                        Status = s.Status,
                        UpdateSystem = s.UpdateSystem,
                        ApprovedBy = s.ApprovedBy,
                        ApprovedByName = s.ApprovedByUser != null ? s.ApprovedByUser.UserName : null,
                        ApprovedAt = s.ApprovedAt,
                        PostedBy = s.PostedBy,
                        PostedByName = s.PostedByUser != null ? s.PostedByUser.UserName : null,
                        PostedAt = s.PostedAt,
                        CreatedAt = s.CreatedAt,
                        UpdatedAt = s.UpdatedAt,
                        Lines = s.Lines.Select(l => new StocktakingLineDto
                        {
                            Id = l.Id,
                            ProductId = l.ProductId,
                            ProductName = l.Product != null ? l.Product.Name : string.Empty,
                            PhysicalQty = l.PhysicalQty,
                            SystemQtySnapshot = l.SystemQtySnapshot,
                            SystemQtyAtPost = l.SystemQtyAtPost,
                            VarianceQty = l.VarianceQty,
                            ValuationCost = l.ValuationCost,
                            VarianceValue = l.VarianceValue,
                            Note = l.Note,
                            ImagePath = l.ImagePath
                        }).ToList()
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                if (stocktaking == null)
                {
                    _logger.LogWarning("Stocktaking with ID {StocktakingId} not found", request.Id);
                    throw new NotFoundException(
                        $"Stocktaking with ID {request.Id} not found",
                        FinanceErrorCode.NotFound);
                }

                _logger.LogInformation("Stocktaking with ID {StocktakingId} fetched successfully", request.Id);

                return ResponseViewModel<StocktakingDetailDto>.Success(
                    stocktaking,
                    "Stocktaking retrieved successfully");
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching stocktaking with ID {StocktakingId}", request.Id);
                throw new Exception($"An error occurred while fetching stocktaking with ID {request.Id}", ex);
            }
        }
    }
}

