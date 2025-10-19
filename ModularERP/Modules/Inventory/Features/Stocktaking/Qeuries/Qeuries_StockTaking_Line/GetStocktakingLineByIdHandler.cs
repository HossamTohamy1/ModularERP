using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Stocktaking.DTO.DTO_StockTaking_Line;
using ModularERP.Modules.Inventory.Features.Stocktaking.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Qeuries.Qeuries_StockTaking_Line
{
    public class GetStocktakingLineByIdHandler : IRequestHandler<GetStocktakingLineByIdQuery, ResponseViewModel<StocktakingLineDto>>
    {
        private readonly IGeneralRepository<StocktakingLine> _lineRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetStocktakingLineByIdHandler> _logger;

        public GetStocktakingLineByIdHandler(
            IGeneralRepository<StocktakingLine> lineRepository,
            IMapper mapper,
            ILogger<GetStocktakingLineByIdHandler> logger)
        {
            _lineRepository = lineRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<StocktakingLineDto>> Handle(
            GetStocktakingLineByIdQuery request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Processing GetStocktakingLineByIdQuery for line {LineId}",
                request.LineId);

            // Get line with projection
            var line = await _lineRepository
                .Get(l => l.Id == request.LineId &&
                         l.StocktakingId == request.StocktakingId)
                .ProjectTo<StocktakingLineDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);

            if (line == null)
            {
                _logger.LogWarning("Stocktaking line {LineId} not found", request.LineId);
                throw new NotFoundException(
                    $"Stocktaking line with ID {request.LineId} not found",
                    FinanceErrorCode.NotFound);
            }

            _logger.LogInformation("Retrieved stocktaking line {LineId} successfully", request.LineId);

            return ResponseViewModel<StocktakingLineDto>.Success(
                line,
                "Stocktaking line retrieved successfully");
        }
    }
}
