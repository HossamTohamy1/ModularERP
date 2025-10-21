using AutoMapper;
using MediatR;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Enum.Inventory_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Stocktaking.Commends.Commands_StockTraking_WorkFlow;
using ModularERP.Modules.Inventory.Features.Stocktaking.DTO.DTO_StockTaking_Header;
using ModularERP.Modules.Inventory.Features.Stocktaking.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Handlers.Handlers_StockTraking_WorkFlow
{
    public class UpdateStocktakingHandler : IRequestHandler<UpdateStocktakingCommand, ResponseViewModel<UpdateStocktakingDto>>
    {
        private readonly IGeneralRepository<StocktakingHeader> _repo;
        private readonly IMapper _mapper;
        private readonly ILogger<UpdateStocktakingHandler> _logger;

        public UpdateStocktakingHandler(
            IGeneralRepository<StocktakingHeader> repo,
            IMapper mapper,
            ILogger<UpdateStocktakingHandler> logger)
        {
            _repo = repo;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<UpdateStocktakingDto>> Handle(
            UpdateStocktakingCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Updating stocktaking {StocktakingId}", request.StocktakingId);

                var entity = await _repo.GetByIDWithTracking(request.StocktakingId);
                if (entity == null)
                    throw new NotFoundException("Stocktaking not found", FinanceErrorCode.NotFound);

                if (entity.Status != StocktakingStatus.Draft)
                    throw new BusinessLogicException("Only draft stocktakings can be updated", "Inventory");

                entity.Notes = request.Notes;
                entity.UpdateSystem = request.UpdateSystem;
                entity.UpdatedAt = DateTime.UtcNow;

                await _repo.Update(entity);

                var result = _mapper.Map<UpdateStocktakingDto>(entity);
                return ResponseViewModel<UpdateStocktakingDto>.Success(result, "Stocktaking updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating stocktaking {StocktakingId}", request.StocktakingId);
                throw;
            }
        }
    }
}