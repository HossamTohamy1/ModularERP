using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Suppliers.Models;
using ModularERP.Modules.Purchases.KPIs.DTO;
using ModularERP.Modules.Purchases.KPIs.Qeuries;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.KPIs.Handlers
{
    public class GetSupplierPerformanceHandler : IRequestHandler<GetSupplierPerformanceQuery, ResponseViewModel<SupplierPerformanceDto>>
    {
        private readonly IGeneralRepository<Supplier> _supplierRepository;
        private readonly IGeneralRepository<PurchaseOrder> _poRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetSupplierPerformanceHandler> _logger;

        public GetSupplierPerformanceHandler(
            IGeneralRepository<Supplier> supplierRepository,
            IGeneralRepository<PurchaseOrder> poRepository,
            IMapper mapper,
            ILogger<GetSupplierPerformanceHandler> logger)
        {
            _supplierRepository = supplierRepository;
            _poRepository = poRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<SupplierPerformanceDto>> Handle(
            GetSupplierPerformanceQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Fetching supplier performance KPI for CompanyId: {CompanyId}", request.CompanyId);

                var startDate = request.StartDate ?? DateTime.UtcNow.AddMonths(-12);
                var endDate = request.EndDate ?? DateTime.UtcNow;

             
                var suppliers = await _supplierRepository.GetByCompanyId(request.CompanyId)
                    .Where(s => s.Status == Common.Enum.Inventory_Enum.SupplierStatus.Active)
                    .Select(s => new
                    {
                        s.Id,
                        s.Name,
                        s.CurrentBalance,
                        s.TotalPurchases
                    })
                    .ToListAsync(cancellationToken);

                var totalActiveSuppliers = suppliers.Count;
                var suppliersWithBalance = suppliers.Count(s => s.CurrentBalance > 0);

            
                var purchaseOrders = await _poRepository.GetByCompanyId(request.CompanyId)
                    .Where(po => po.PODate >= startDate &&
                                 po.PODate <= endDate &&
                                 po.DocumentStatus != "Cancelled")
                    .Select(po => new
                    {
                        po.SupplierId,
                        po.TotalAmount,
                        po.ReceptionStatus,
                        po.ApprovedAt,
                        po.ClosedAt
                    })
                    .ToListAsync(cancellationToken);

                var supplierStats = purchaseOrders
                    .GroupBy(po => po.SupplierId)
                    .Select(g =>
                    {
                        var supplier = suppliers.FirstOrDefault(s => s.Id == g.Key);
                        var totalOrders = g.Count();
                        var onTimeDeliveries = g.Count(po =>
                            po.ReceptionStatus == "FullyReceived" &&
                            po.ClosedAt.HasValue &&
                            po.ApprovedAt.HasValue &&
                            (po.ClosedAt.Value - po.ApprovedAt.Value).TotalDays <= 7);

                        return new TopSupplierDto
                        {
                            SupplierId = g.Key,
                            SupplierName = supplier?.Name ?? "Unknown",
                            TotalPurchaseAmount = g.Sum(po => po.TotalAmount),
                            OrderCount = totalOrders,
                            CurrentBalance = supplier?.CurrentBalance ?? 0,
                            OnTimeDeliveryRate = totalOrders > 0 ? (decimal)onTimeDeliveries / totalOrders * 100 : 0
                        };
                    })
                    .OrderByDescending(s => s.TotalPurchaseAmount)
                    .Take(request.TopCount)
                    .ToList();

                var result = new SupplierPerformanceDto
                {
                    TopSuppliers = supplierStats,
                    AverageSupplierRating = supplierStats.Any()
                        ? Math.Round(supplierStats.Average(s => s.OnTimeDeliveryRate), 2)
                        : 0,
                    TotalActiveSuppliers = totalActiveSuppliers,
                    SuppliersWithOutstandingBalance = suppliersWithBalance
                };

                _logger.LogInformation("Supplier performance retrieved. Active suppliers: {Count}", totalActiveSuppliers);

                return ResponseViewModel<SupplierPerformanceDto>.Success(result, "Supplier performance retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving supplier performance for CompanyId: {CompanyId}", request.CompanyId);
                throw new BusinessLogicException(
                    "Failed to retrieve supplier performance",
                    "Purchases",
                    Common.Enum.Finance_Enum.FinanceErrorCode.DatabaseError);
            }
        }
    }
}