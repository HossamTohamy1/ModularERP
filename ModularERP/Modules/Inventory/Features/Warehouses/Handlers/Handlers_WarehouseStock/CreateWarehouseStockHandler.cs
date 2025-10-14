using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Finance.Infrastructure.Data;
using ModularERP.Modules.Inventory.Features.Warehouses.Commends.Commends_WarehouseStock;
using ModularERP.Modules.Inventory.Features.Warehouses.DTO.DTO_WarehouseStock;
using ModularERP.Modules.Inventory.Features.Warehouses.Models;
using ModularERP.Shared.Interfaces;
using Serilog;
using ILogger = Serilog.ILogger;

namespace ModularERP.Modules.Inventory.Features.Warehouses.Handlers.Handlers_WarehouseStock
{
    public class CreateWarehouseStockHandler : IRequestHandler<CreateWarehouseStockCommand, ResponseViewModel<WarehouseStockResponseDto>>
    {
        private readonly IGeneralRepository<WarehouseStock> _repository;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;
        private readonly FinanceDbContext _dbContext;

        public CreateWarehouseStockHandler(
            IGeneralRepository<WarehouseStock> repository,
            IMapper mapper,
            FinanceDbContext dbContext)
        {
            _repository = repository;
            _mapper = mapper;
            _dbContext = dbContext;
            _logger = Log.ForContext<CreateWarehouseStockHandler>();
        }

        public async Task<ResponseViewModel<WarehouseStockResponseDto>> Handle(
            CreateWarehouseStockCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.Information("Creating warehouse stock for Warehouse: {WarehouseId}, Product: {ProductId}",
                    request.Data.WarehouseId, request.Data.ProductId);

                // Check if already exists
                var exists = await _repository.AnyAsync(
                    x => x.WarehouseId == request.Data.WarehouseId
                      && x.ProductId == request.Data.ProductId
                      && !x.IsDeleted,
                    cancellationToken);

                if (exists)
                {
                    _logger.Warning("Warehouse stock already exists for Warehouse: {WarehouseId}, Product: {ProductId}",
                        request.Data.WarehouseId, request.Data.ProductId);
                    throw new BusinessLogicException(
                        "Stock record already exists for this warehouse and product",
                        "Inventory",
                        FinanceErrorCode.BusinessLogicError);
                }

                var entity = _mapper.Map<WarehouseStock>(request.Data);
                entity.Id = Guid.NewGuid();
                entity.CreatedAt = DateTime.UtcNow;
                entity.CreatedById = Guid.Empty; // TODO: Get from current user

                await _repository.AddAsync(entity);
                await _repository.SaveChanges();

                _logger.Information("Warehouse stock created successfully with Id: {Id}", entity.Id);

                // Use Projection to get related data
                var result = await _dbContext.Set<WarehouseStock>()
                    .Where(x => x.Id == entity.Id)
                    .ProjectTo<WarehouseStockResponseDto>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync(cancellationToken);

                if (result == null)
                {
                    throw new NotFoundException(
                        $"Warehouse stock with ID {entity.Id} not found after creation",
                        FinanceErrorCode.NotFound);
                }

                // Get warehouse and product names separately
                var warehouse = await _dbContext.Set<Warehouse>()
                    .Where(w => w.Id == entity.WarehouseId)
                    .Select(w => w.Name)
                    .FirstOrDefaultAsync(cancellationToken);

                var product = await _dbContext.Set<Product>()
                    .Where(p => p.Id == entity.ProductId)
                    .Select(p => new { p.Name, p.SKU })
                    .FirstOrDefaultAsync(cancellationToken);

                result.WarehouseName = warehouse ?? string.Empty;
                result.ProductName = product?.Name ?? string.Empty;
                result.ProductSKU = product?.SKU ?? string.Empty;

                return ResponseViewModel<WarehouseStockResponseDto>.Success(
                    result,
                    "Warehouse stock created successfully");
            }
            catch (Exception ex) when (ex is not BusinessLogicException && ex is not NotFoundException)
            {
                _logger.Error(ex, "Error creating warehouse stock");
                throw;
            }
        }
    }
}
