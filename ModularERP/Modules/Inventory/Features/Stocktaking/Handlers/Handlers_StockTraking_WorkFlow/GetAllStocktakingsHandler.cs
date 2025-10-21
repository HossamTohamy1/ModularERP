using AutoMapper;
using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Stocktaking.DTO.DTO_StockTaking_Header;
using ModularERP.Modules.Inventory.Features.Stocktaking.Models;
using ModularERP.Modules.Inventory.Features.Stocktaking.Qeuries.Qeuries_StockTraking_WorkFlow;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Handlers.Handlers_StockTraking_WorkFlow
{
    public class GetAllStocktakingsHandler : IRequestHandler<GetAllStocktakingsQuery, ResponseViewModel<List<StocktakingListDto>>>
    {
        private readonly IGeneralRepository<StocktakingHeader> _stocktakingRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<DeleteStocktakingHandler> _logger;

        public GetAllStocktakingsHandler(
            IGeneralRepository<StocktakingHeader> stocktakingRepo,
            IMapper mapper,
            ILogger<DeleteStocktakingHandler> logger)
        {
            _stocktakingRepo = stocktakingRepo;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<List<StocktakingListDto>>> Handle(
            GetAllStocktakingsQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Fetching stocktakings for company {CompanyId}", request.CompanyId);

                var query = _stocktakingRepo.GetByCompanyId(request.CompanyId);

                if (request.WarehouseId.HasValue)
                    query = query.Where(s => s.WarehouseId == request.WarehouseId.Value);

                if (request.Status.HasValue)
                    query = query.Where(s => s.Status == request.Status.Value);

                if (request.FromDate.HasValue)
                    query = query.Where(s => s.DateTime >= request.FromDate.Value);

                if (request.ToDate.HasValue)
                    query = query.Where(s => s.DateTime <= request.ToDate.Value);

                var items = query
                    .OrderByDescending(s => s.CreatedAt)
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Select(s => _mapper.Map<StocktakingListDto>(s))
                    .ToList();

                return ResponseViewModel<List<StocktakingListDto>>.Success(items, "Stocktakings retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching stocktakings for company {CompanyId}", request.CompanyId);
                throw;
            }
        }
    }
}