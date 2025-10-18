using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Inventory_Enum;
using ModularERP.Modules.Inventory.Features.Products.Models;
using ModularERP.Modules.Inventory.Features.Requisitions.Models;
using ModularERP.Modules.Inventory.Features.Warehouses.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.Requisitions.Services
{


        public class ProductStatsService : IProductStatsService
        {
            private readonly IGeneralRepository<ProductStats> _productStatsRepo;
            private readonly IGeneralRepository<WarehouseStock> _warehouseStockRepo;
            private readonly IGeneralRepository<RequisitionItem> _requisitionItemRepo;

            public ProductStatsService(
                IGeneralRepository<ProductStats> productStatsRepo,
                IGeneralRepository<WarehouseStock> warehouseStockRepo,
                IGeneralRepository<RequisitionItem> requisitionItemRepo)
            {
                _productStatsRepo = productStatsRepo;
                _warehouseStockRepo = warehouseStockRepo;
                _requisitionItemRepo = requisitionItemRepo;
            }

            public async Task UpdateProductStats(Guid productId, Guid companyId, CancellationToken cancellationToken = default)
            {
                // جلب أو إنشاء ProductStats
                var productStats = await _productStatsRepo
                    .GetAll()
                    .FirstOrDefaultAsync(ps => ps.ProductId == productId && ps.CompanyId == companyId, cancellationToken);

                if (productStats == null)
                {
                    productStats = new ProductStats
                    {
                        ProductId = productId,
                        CompanyId = companyId,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _productStatsRepo.AddAsync(productStats);
                }

                // 1. حساب إجمالي المخزون (OnHandStock) من جميع المخازن
                var totalStock = await _warehouseStockRepo
                    .GetAll()
                    .Where(ws => ws.ProductId == productId)
                    .SumAsync(ws => ws.Quantity, cancellationToken);

                productStats.OnHandStock = totalStock;

                // 2. حساب متوسط التكلفة (AvgUnitCost) من المخازن
                var warehouseStocks = await _warehouseStockRepo
                    .GetAll()
                    .Where(ws => ws.ProductId == productId && ws.Quantity > 0)
                    .ToListAsync(cancellationToken);

                if (warehouseStocks.Any())
                {
                    decimal totalValue = 0;
                    decimal totalQuantity = 0;

                    foreach (var stock in warehouseStocks)
                    {
                        if (stock.AverageUnitCost.HasValue)
                        {
                            totalValue += stock.Quantity * stock.AverageUnitCost.Value;
                            totalQuantity += stock.Quantity;
                        }
                    }

                    productStats.AvgUnitCost = totalQuantity > 0 ? totalValue / totalQuantity : 0;
                }

                // 3. حساب المبيعات (Outbound Requisitions فقط)
                var now = DateTime.UtcNow;
                var last28Days = now.AddDays(-28);
                var last7Days = now.AddDays(-7);

                // إجمالي المبيعات (كل الـ Outbound المؤكدة)
                var totalSold = await _requisitionItemRepo
                    .GetAll()
                    .Include(ri => ri.Requisition)
                    .Where(ri => ri.ProductId == productId
                              && ri.Requisition.CompanyId == companyId
                              && ri.Requisition.Type == RequisitionType.Outbound
                              && ri.Requisition.Status == RequisitionStatus.Confirmed)
                    .SumAsync(ri => ri.Quantity, cancellationToken);

                productStats.TotalSold = totalSold;

                // المبيعات آخر 28 يوم
                var soldLast28Days = await _requisitionItemRepo
                    .GetAll()
                    .Include(ri => ri.Requisition)
                    .Where(ri => ri.ProductId == productId
                              && ri.Requisition.CompanyId == companyId
                              && ri.Requisition.Type == RequisitionType.Outbound
                              && ri.Requisition.Status == RequisitionStatus.Confirmed
                              && ri.Requisition.ConfirmedAt >= last28Days)
                    .SumAsync(ri => ri.Quantity, cancellationToken);

                productStats.SoldLast28Days = soldLast28Days;

                // المبيعات آخر 7 أيام
                var soldLast7Days = await _requisitionItemRepo
                    .GetAll()
                    .Include(ri => ri.Requisition)
                    .Where(ri => ri.ProductId == productId
                              && ri.Requisition.CompanyId == companyId
                              && ri.Requisition.Type == RequisitionType.Outbound
                              && ri.Requisition.Status == RequisitionStatus.Confirmed
                              && ri.Requisition.ConfirmedAt >= last7Days)
                    .SumAsync(ri => ri.Quantity, cancellationToken);

                productStats.SoldLast7Days = soldLast7Days;

                // تحديث التاريخ
                productStats.LastUpdated = DateTime.UtcNow;
                productStats.UpdatedAt = DateTime.UtcNow;

                await _productStatsRepo.Update(productStats);
                await _productStatsRepo.SaveChanges();
            }
        }
    }