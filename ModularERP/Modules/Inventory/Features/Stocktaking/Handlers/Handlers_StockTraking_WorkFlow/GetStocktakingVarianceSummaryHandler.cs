using AutoMapper;
using MediatR;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Stocktaking.DTO.DTO_StockTaking_WorkFlow;
using ModularERP.Modules.Inventory.Features.Stocktaking.Models;
using ModularERP.Modules.Inventory.Features.Stocktaking.Qeuries.Qeuries_StockTraking_WorkFlow;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Handlers.Handlers_StockTraking_WorkFlow
{
    public class GetStocktakingVarianceSummaryHandler : IRequestHandler<GetStocktakingVarianceSummaryQuery, ResponseViewModel<VarianceSummaryDto>>
    {
        private readonly IGeneralRepository<StocktakingHeader> _stocktakingRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<GetStocktakingVarianceSummaryHandler> _logger;

        public GetStocktakingVarianceSummaryHandler(
            IGeneralRepository<StocktakingHeader> stocktakingRepo,
            IMapper mapper,
            ILogger<GetStocktakingVarianceSummaryHandler> logger)
        {
            _stocktakingRepo = stocktakingRepo;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<VarianceSummaryDto>> Handle(
            GetStocktakingVarianceSummaryQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Fetching variance summary for stocktaking {StocktakingId}", request.StocktakingId);

                var stocktaking = await _stocktakingRepo.GetByID(request.StocktakingId);
                if (stocktaking == null)
                    throw new NotFoundException("Stocktaking not found", FinanceErrorCode.NotFound);

                if (stocktaking.CompanyId != request.CompanyId)
                    throw new BusinessLogicException("Unauthorized access", "Inventory", FinanceErrorCode.UnauthorizedAccess);

                var summary = new VarianceSummaryDto
                {
                    StocktakingId = stocktaking.Id,
                    Number = stocktaking.Number,
                    Status = stocktaking.Status.ToString(),
                    TotalRecordedItems = stocktaking.Lines.Count,
                    TotalShortages = stocktaking.Lines.Count(l => (l.VarianceQty ?? 0) < 0),
                    TotalOverages = stocktaking.Lines.Count(l => (l.VarianceQty ?? 0) > 0),
                    TotalShortageQty = stocktaking.Lines.Where(l => (l.VarianceQty ?? 0) < 0).Sum(l => Math.Abs(l.VarianceQty ?? 0)),
                    TotalOverageQty = stocktaking.Lines.Where(l => (l.VarianceQty ?? 0) > 0).Sum(l => l.VarianceQty ?? 0),
                    TotalShortageValue = stocktaking.Lines
                        .Where(l => (l.VarianceQty ?? 0) < 0)
                        .Sum(l => Math.Abs((l.VarianceValue ?? 0))),
                    TotalOverageValue = stocktaking.Lines
                        .Where(l => (l.VarianceQty ?? 0) > 0)
                        .Sum(l => (l.VarianceValue ?? 0)),
                    CreatedAt = stocktaking.CreatedAt,
                    CreatedByName = stocktaking.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
                };

                return ResponseViewModel<VarianceSummaryDto>.Success(summary, "Variance summary retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching variance summary for stocktaking {StocktakingId}", request.StocktakingId);
                throw;
            }
        }
    }

}
