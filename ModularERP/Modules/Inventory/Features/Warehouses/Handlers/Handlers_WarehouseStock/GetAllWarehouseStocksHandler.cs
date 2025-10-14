using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Finance.Infrastructure.Data;
using ModularERP.Modules.Inventory.Features.Warehouses.Commends.Commends_WarehouseStock;
using ModularERP.Modules.Inventory.Features.Warehouses.DTO.DTO_WarehouseStock;
using ModularERP.Modules.Inventory.Features.Warehouses.Models;
using Serilog;
using ILogger = Serilog.ILogger;

namespace ModularERP.Modules.Inventory.Features.Warehouses.Handlers.Handlers_WarehouseStock
{
    public class GetAllWarehouseStocksHandler : IRequestHandler<GetAllWarehouseStocksQuery, ResponseViewModel<List<WarehouseStockListDto>>>
    {
        private readonly IMapper _mapper;
        private readonly ILogger _logger;
        private readonly FinanceDbContext _dbContext;

        public GetAllWarehouseStocksHandler(IMapper mapper, FinanceDbContext dbContext)
        {
            _mapper = mapper;
            _dbContext = dbContext;
            _logger = Log.ForContext<GetAllWarehouseStocksHandler>();
        }

        public async Task<ResponseViewModel<List<WarehouseStockListDto>>> Handle(
            GetAllWarehouseStocksQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.Information("Getting all warehouse stocks");

                var query = _dbContext.Set<WarehouseStock>()
                    .Where(x => !x.IsDeleted);

                if (request.WarehouseId.HasValue)
                    query = query.Where(x => x.WarehouseId == request.WarehouseId.Value);

                if (request.ProductId.HasValue)
                    query = query.Where(x => x.ProductId == request.ProductId.Value);

                if (request.LowStockOnly == true)
                    query = query.Where(x => x.MinStockLevel.HasValue && x.Quantity <= x.MinStockLevel.Value);

                var results = await query
                    .ProjectTo<WarehouseStockListDto>(_mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);

                // Get warehouse and product names
                foreach (var item in results)
                {
                    var stock = await _dbContext.Set<WarehouseStock>()
                        .Where(s => s.Id == item.Id)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (stock != null)
                    {
                        var warehouse = await _dbContext.Set<Warehouse>()
                            .Where(w => w.Id == stock.WarehouseId)
                            .Select(w => w.Name)
                            .FirstOrDefaultAsync(cancellationToken);

                        var product = await _dbContext.Set<Product>()
                            .Where(p => p.Id == stock.ProductId)
                            .Select(p => new { p.Name, p.SKU })
                            .FirstOrDefaultAsync(cancellationToken);

                        item.WarehouseName = warehouse ?? string.Empty;
                        item.ProductName = product?.Name ?? string.Empty;
                        item.ProductSKU = product?.SKU ?? string.Empty;
                    }
                }

                _logger.Information("Retrieved {Count} warehouse stocks", results.Count);

                return ResponseViewModel<List<WarehouseStockListDto>>.Success(
                    results,
                    "Warehouse stocks retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting all warehouse stocks");
                throw;
            }
        }
    }
}
