using AutoMapper;
using MediatR;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Stocktaking.DTO.DTO_StockTaking_Header;
using ModularERP.Modules.Inventory.Features.Stocktaking.Models;
using ModularERP.Modules.Inventory.Features.Stocktaking.Qeuries.Qeuries_StockTraking_WorkFlow;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Handlers.Handlers_StockTraking_WorkFlow
{
    public class GetStocktakingDetailHandler : IRequestHandler<GetStocktakingDetailQuery, ResponseViewModel<StocktakingDetailDto>>
    {
        private readonly IGeneralRepository<StocktakingHeader> _stocktakingRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<GetStocktakingDetailHandler> _logger;

        public GetStocktakingDetailHandler(
            IGeneralRepository<StocktakingHeader> stocktakingRepo,
            IMapper mapper,
            ILogger<GetStocktakingDetailHandler> logger)
        {
            _stocktakingRepo = stocktakingRepo;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<StocktakingDetailDto>> Handle(
            GetStocktakingDetailQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Fetching stocktaking detail {StocktakingId}", request.StocktakingId);

                var stocktaking = await _stocktakingRepo.GetByID(request.StocktakingId);
                if (stocktaking == null)
                    throw new NotFoundException("Stocktaking not found", FinanceErrorCode.NotFound);

                if (stocktaking.CompanyId != request.CompanyId)
                    throw new BusinessLogicException("Unauthorized access", "Inventory", FinanceErrorCode.UnauthorizedAccess);

                var result = _mapper.Map<StocktakingDetailDto>(stocktaking);
                return ResponseViewModel<StocktakingDetailDto>.Success(result, "Stocktaking detail retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching stocktaking detail {StocktakingId}", request.StocktakingId);
                throw;
            }
        }
    }

}
