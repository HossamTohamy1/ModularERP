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
    public class UpdateWarehouseStockHandler : IRequestHandler<UpdateWarehouseStockCommand, ResponseViewModel<WarehouseStockResponseDto>>
    {
        private readonly IGeneralRepository<WarehouseStock> _repository;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;
        private readonly FinanceDbContext _dbContext;

        public UpdateWarehouseStockHandler(
            IGeneralRepository<WarehouseStock> repository,
            IMapper mapper,
            FinanceDbContext dbContext)
        {
            _repository = repository;
            _mapper = mapper;
            _dbContext = dbContext;
            _logger = Log.ForContext<UpdateWarehouseStockHandler>();
        }

        public async Task<ResponseViewModel<WarehouseStockResponseDto>> Handle(
            UpdateWarehouseStockCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.Information("Updating warehouse stock with Id: {Id}", request.Id);

                var entity = await _repository.GetByID(request.Id);
                if (entity == null || entity.IsDeleted)
                {
                    _logger.Warning("Warehouse stock not found with Id: {Id}", request.Id);
                    throw new NotFoundException(
                        $"Warehouse stock with ID {request.Id} not found",
                        FinanceErrorCode.NotFound);
                }

                _mapper.Map(request.Data, entity);
                entity.UpdatedAt = DateTime.UtcNow;
                entity.UpdatedById = Guid.Empty; // TODO: Get from current user

                _repository.Update(entity);
                await _repository.SaveChanges();

                _logger.Information("Warehouse stock updated successfully with Id: {Id}", request.Id);

                var result = await _dbContext.Set<WarehouseStock>()
                    .Where(x => x.Id == entity.Id)
                    .ProjectTo<WarehouseStockResponseDto>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync(cancellationToken);

                if (result == null)
                {
                    throw new NotFoundException(
                        $"Warehouse stock with ID {entity.Id} not found after update",
                        FinanceErrorCode.NotFound);
                }

                // Get warehouse and product names
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
                    "Warehouse stock updated successfully");
            }
            catch (Exception ex) when (ex is not BusinessLogicException && ex is not NotFoundException)
            {
                _logger.Error(ex, "Error updating warehouse stock with Id: {Id}", request.Id);
                throw;
            }
        }
    }

}
