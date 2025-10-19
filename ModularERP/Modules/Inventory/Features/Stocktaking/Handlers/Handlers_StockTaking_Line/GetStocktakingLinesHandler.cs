using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Stocktaking.DTO.DTO_StockTaking_Line;
using ModularERP.Modules.Inventory.Features.Stocktaking.Models;
using ModularERP.Modules.Inventory.Features.Stocktaking.Qeuries.Qeuries_StockTaking_Line;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Handlers.Handlers_StockTaking_Line
{
    public class GetStocktakingLinesHandler : IRequestHandler<GetStocktakingLinesQuery, ResponseViewModel<List<StocktakingLineDto>>>
    {
        private readonly IGeneralRepository<StocktakingLine> _lineRepository;
        private readonly IGeneralRepository<StocktakingHeader> _headerRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetStocktakingLinesHandler> _logger;

        public GetStocktakingLinesHandler(
            IGeneralRepository<StocktakingLine> lineRepository,
            IGeneralRepository<StocktakingHeader> headerRepository,
            IMapper mapper,
            ILogger<GetStocktakingLinesHandler> logger)
        {
            _lineRepository = lineRepository;
            _headerRepository = headerRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<List<StocktakingLineDto>>> Handle(
            GetStocktakingLinesQuery request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Processing GetStocktakingLinesQuery for stocktaking {StocktakingId}",
                request.StocktakingId);

            // Validate stocktaking exists
            var stocktakingExists = await _headerRepository
                .AnyAsync(s => s.Id == request.StocktakingId, cancellationToken);

            if (!stocktakingExists)
            {
                _logger.LogWarning("Stocktaking {StocktakingId} not found", request.StocktakingId);
                throw new NotFoundException(
                    $"Stocktaking with ID {request.StocktakingId} not found",
                    FinanceErrorCode.NotFound);
            }

            // Get all lines with projection
            var lines = await _lineRepository
                .Get(l => l.StocktakingId == request.StocktakingId)
                .OrderBy(l => l.CreatedAt)
                .ProjectTo<StocktakingLineDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            _logger.LogInformation("Retrieved {Count} lines for stocktaking {StocktakingId}",
                lines.Count, request.StocktakingId);

            return ResponseViewModel<List<StocktakingLineDto>>.Success(
                lines,
                $"Retrieved {lines.Count} stocktaking lines successfully");
        }
    }

}