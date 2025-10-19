using AutoMapper;
using MediatR;
using ModularERP.Common.Enum.Inventory_Enum;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Stocktaking.DTO.DTO_StockTaking_Header;
using ModularERP.Modules.Inventory.Features.Stocktaking.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Handlers.Handlers_StockTraking_WorkFlow
{
    public class CreateStocktakingHandler : IRequestHandler<CreateStocktakingCommand, ResponseViewModel<CreateStocktakingDto>>
    {
        private readonly IGeneralRepository<StocktakingHeader> _repo;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public CreateStocktakingHandler(
            IGeneralRepository<StocktakingHeader> repo,
            IMapper mapper,
            ILogger logger)
        {
            _repo = repo;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<CreateStocktakingDto>> Handle(
            CreateStocktakingCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.Information("Creating stocktaking {Number} for warehouse {WarehouseId}",
                    request.Number, request.WarehouseId);

                var entity = new StocktakingHeader
                {
                    Id = Guid.NewGuid(),
                    WarehouseId = request.WarehouseId,
                    CompanyId = request.CompanyId,
                    Number = request.Number,
                    DateTime = request.DateTime,
                    Notes = request.Notes,
                    Status = StocktakingStatus.Draft,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false
                };

                await _repo.AddAsync(entity);
                await _repo.SaveChanges();

                var result = _mapper.Map<CreateStocktakingDto>(entity);
                return ResponseViewModel<CreateStocktakingDto>.Success(result, "Stocktaking created successfully");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error creating stocktaking");
                throw;
            }
        }
    }
}