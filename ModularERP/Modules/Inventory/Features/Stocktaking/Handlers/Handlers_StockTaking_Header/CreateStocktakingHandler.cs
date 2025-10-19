using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Inventory_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Stocktaking.Commends.Commends_StockTaking_Header;
using ModularERP.Modules.Inventory.Features.Stocktaking.DTO.DTO_StockTaking_Header;
using ModularERP.Modules.Inventory.Features.Stocktaking.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Handlers.Handlers_StockTaking_Header
{
    public class CreateStocktakingHandler : IRequestHandler<CreateStocktakingCommand, ResponseViewModel<CreateStocktakingDto>>
    {
        private readonly IGeneralRepository<StocktakingHeader> _repo;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateStocktakingHandler> _logger;

    public CreateStocktakingHandler(
        IGeneralRepository<StocktakingHeader> repo,
        IMapper mapper,
        ILogger<CreateStocktakingHandler> logger)
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
            _logger.LogInformation("Creating stocktaking with number {Number} for warehouse {WarehouseId}",
                request.Number, request.WarehouseId);

            // Check if number already exists
            var existingStocktaking = await _repo
                .Get(s => s.Number == request.Number && s.CompanyId == request.CompanyId)
                .FirstOrDefaultAsync(cancellationToken);

            if (existingStocktaking != null)
            {
                _logger.LogWarning("Stocktaking number {Number} already exists", request.Number);
                throw new BusinessLogicException(
                    $"Stocktaking number '{request.Number}' already exists",
                    "Inventory",
                    Common.Enum.Finance_Enum.FinanceErrorCode.DuplicateEntity);
            }

            var stocktaking = new StocktakingHeader
            {
                Id = Guid.NewGuid(),
                WarehouseId = request.WarehouseId,
                CompanyId = request.CompanyId,
                Number = request.Number,
                DateTime = request.DateTime,
                Notes = request.Notes,
                Status = StocktakingStatus.Draft,
                UpdateSystem = request.UpdateSystem,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _repo.AddAsync(stocktaking);
            await _repo.SaveChanges();

            _logger.LogInformation("Stocktaking created successfully with ID {StocktakingId}", stocktaking.Id);

            // Projection using AutoMapper
            var dto = await _repo
                .Get(s => s.Id == stocktaking.Id)
                .Select(s => new CreateStocktakingDto
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
                    CreatedAt = s.CreatedAt
                })
                .FirstOrDefaultAsync(cancellationToken);

            return ResponseViewModel<CreateStocktakingDto>.Success(
                dto!,
                "Stocktaking created successfully");
        }
        catch (BusinessLogicException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating stocktaking with number {Number}", request.Number);
            throw new BusinessLogicException(
                "An error occurred while creating stocktaking",
                ex,
                "Inventory");
        }
    }
}
}