using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Enum.Inventory_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Stocktaking.Commends.Commends_StockTaking_Header;
using ModularERP.Modules.Inventory.Features.Stocktaking.DTO.DTO_StockTaking_Header;
using ModularERP.Modules.Inventory.Features.Stocktaking.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Handlers.Handlers_StockTaking_Header
{
    public class UpdateStocktakingHandler : IRequestHandler<UpdateStocktakingCommand, ResponseViewModel<UpdateStocktakingDto>>
    {
        private readonly IGeneralRepository<StocktakingHeader> _repo;
        private readonly ILogger<UpdateStocktakingHandler> _logger;

        public UpdateStocktakingHandler(
            IGeneralRepository<StocktakingHeader> repo,
            ILogger<UpdateStocktakingHandler> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<ResponseViewModel<UpdateStocktakingDto>> Handle(
            UpdateStocktakingCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Updating stocktaking with ID {StocktakingId}", request.Id);

                var stocktaking = await _repo.GetByIDWithTracking(request.Id);

                if (stocktaking == null)
                {
                    _logger.LogWarning("Stocktaking with ID {StocktakingId} not found", request.Id);
                    throw new NotFoundException(
                        $"Stocktaking with ID {request.Id} not found",
                        FinanceErrorCode.NotFound);
                }

                if (stocktaking.Status != StocktakingStatus.Draft)
                {
                    _logger.LogWarning("Cannot update stocktaking {StocktakingId} with status {Status}",
                        request.Id, stocktaking.Status);
                    throw new BusinessLogicException(
                        "Only Draft stocktakings can be updated",
                        "Inventory",
                        FinanceErrorCode.BusinessLogicError);
                }

                // Update properties
                stocktaking.WarehouseId = request.WarehouseId;
                stocktaking.Number = request.Number;
                stocktaking.DateTime = request.DateTime;
                stocktaking.Notes = request.Notes;
                stocktaking.UpdateSystem = request.UpdateSystem;
                stocktaking.UpdatedAt = DateTime.UtcNow;

                await _repo.SaveChanges();

                _logger.LogInformation("Stocktaking {StocktakingId} updated successfully", request.Id);

                // Projection
                var dto = await _repo
                    .Get(s => s.Id == stocktaking.Id)
                    .Select(s => new UpdateStocktakingDto
                    {
                        Id = s.Id,
                        WarehouseId = s.WarehouseId,
                        WarehouseName = s.Warehouse != null ? s.Warehouse.Name : string.Empty,
                        Number = s.Number,
                        DateTime = s.DateTime,
                        Notes = s.Notes,
                        Status = s.Status,
                        UpdateSystem = s.UpdateSystem,
                        UpdatedAt = s.UpdatedAt
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                return ResponseViewModel<UpdateStocktakingDto>.Success(
                    dto!,
                    "Stocktaking updated successfully");
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
                _logger.LogError(ex, "Error updating stocktaking with ID {StocktakingId}", request.Id);
                throw new BusinessLogicException(
                    "An error occurred while updating stocktaking",
                    ex,
                    "Inventory");
            }
        }
    }
}