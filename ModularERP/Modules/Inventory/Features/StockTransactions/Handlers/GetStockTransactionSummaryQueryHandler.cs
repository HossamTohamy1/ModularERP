using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Inventory_Enum;
using ModularERP.Modules.Inventory.Features.StockTransactions.DTO;
using ModularERP.Modules.Inventory.Features.StockTransactions.Models;
using ModularERP.Modules.Inventory.Features.StockTransactions.Qeuries;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.StockTransactions.Handlers
{
    public class GetStockTransactionSummaryQueryHandler : IRequestHandler<GetStockTransactionSummaryQuery, List<StockTransactionSummaryDto>>
    {
        private readonly IGeneralRepository<StockTransaction> _repository;

        public GetStockTransactionSummaryQueryHandler(IGeneralRepository<StockTransaction> repository)
        {
            _repository = repository;
        }

        public async Task<List<StockTransactionSummaryDto>> Handle(GetStockTransactionSummaryQuery request, CancellationToken cancellationToken)
        {
            var query = _repository.GetAll();

            if (request.CompanyId.HasValue)
                query = query.Where(x => x.CompanyId == request.CompanyId.Value);

            if (request.WarehouseId.HasValue)
                query = query.Where(x => x.WarehouseId == request.WarehouseId.Value);

            if (request.ProductId.HasValue)
                query = query.Where(x => x.ProductId == request.ProductId.Value);

            if (request.FromDate.HasValue)
                query = query.Where(x => x.CreatedAt >= request.FromDate.Value);

            if (request.ToDate.HasValue)
                query = query.Where(x => x.CreatedAt <= request.ToDate.Value);

            var summary = await query
                .GroupBy(x => new { x.ProductId, x.WarehouseId })
                .Select(g => new StockTransactionSummaryDto
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Select(x => x.Product.Name).FirstOrDefault(),
                    WarehouseId = g.Key.WarehouseId,
                    WarehouseName = g.Select(x => x.Warehouse.Name).FirstOrDefault(),
                    TotalIn = g.Where(x => x.TransactionType == StockTransactionType.Adjustment ||
                                          x.TransactionType == StockTransactionType.Purchase ||
                                          x.TransactionType == StockTransactionType.Return)
                              .Sum(x => x.Quantity),
                    TotalOut = g.Where(x => x.TransactionType == StockTransactionType.Sale ||
                                           x.TransactionType == StockTransactionType.Transfer)
                               .Sum(x => x.Quantity),
                    NetMovement = g.Sum(x => x.TransactionType == StockTransactionType.Sale ||
                                            x.TransactionType == StockTransactionType.Transfer
                                            ? -x.Quantity : x.Quantity),
                    CurrentStock = g.OrderByDescending(x => x.CreatedAt).Select(x => x.StockLevelAfter).FirstOrDefault(),
                    AverageUnitCost = g.Sum(x => (x.UnitCost ?? 0) * x.Quantity) / g.Sum(x => x.Quantity),
                    TransactionCount = g.Count()
                })
                .ToListAsync(cancellationToken);


            return summary;
        }
    }
}
