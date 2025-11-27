using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Purchases_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.KPIs.DTO;
using ModularERP.Modules.Purchases.KPIs.Qeuries;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.KPIs.Handlers
{
    public class GetPurchaseVolumeHandler : IRequestHandler<GetPurchaseVolumeQuery, ResponseViewModel<PurchaseVolumeDto>>
    {
        private readonly IGeneralRepository<PurchaseOrder> _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetPurchaseVolumeHandler> _logger;

        public GetPurchaseVolumeHandler(
            IGeneralRepository<PurchaseOrder> repository,
            IMapper mapper,
            ILogger<GetPurchaseVolumeHandler> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<PurchaseVolumeDto>> Handle(
            GetPurchaseVolumeQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Fetching purchase volume KPI for CompanyId: {CompanyId}", request.CompanyId);

                var startDate = request.StartDate ?? DateTime.UtcNow.AddMonths(-12);
                var endDate = request.EndDate ?? DateTime.UtcNow;

                var query = _repository.GetByCompanyId(request.CompanyId)
                    .Where(po => po.PODate >= startDate && po.PODate <= endDate &&
                                 po.DocumentStatus != DocumentStatus.Cancelled);

                var purchaseOrders = await query
                    .Select(po => new
                    {
                        po.TotalAmount,
                        po.PODate
                    })
                    .ToListAsync(cancellationToken);

                if (!purchaseOrders.Any())
                {
                    _logger.LogInformation("No purchase orders found for the specified period");
                    return ResponseViewModel<PurchaseVolumeDto>.Success(new PurchaseVolumeDto());
                }

                var totalAmount = purchaseOrders.Sum(po => po.TotalAmount);
                var totalCount = purchaseOrders.Count;
                var avgValue = totalCount > 0 ? totalAmount / totalCount : 0;

                var monthlyData = purchaseOrders
                    .GroupBy(po => new { po.PODate.Year, po.PODate.Month })
                    .Select(g => new MonthlyVolumeDto
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        MonthName = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMMM yyyy"),
                        TotalAmount = g.Sum(po => po.TotalAmount),
                        OrderCount = g.Count()
                    })
                    .OrderBy(m => m.Year).ThenBy(m => m.Month)
                    .ToList();

                var growthPercentage = 0m;
                if (monthlyData.Count >= 2)
                {
                    var lastMonth = monthlyData[^1].TotalAmount;
                    var previousMonth = monthlyData[^2].TotalAmount;
                    if (previousMonth > 0)
                    {
                        growthPercentage = ((lastMonth - previousMonth) / previousMonth) * 100;
                    }
                }

                var result = new PurchaseVolumeDto
                {
                    TotalPurchaseAmount = totalAmount,
                    TotalPurchaseOrders = totalCount,
                    AveragePurchaseValue = avgValue,
                    MonthlyGrowthPercentage = Math.Round(growthPercentage, 2),
                    MonthlyBreakdown = monthlyData
                };

                _logger.LogInformation("Purchase volume KPI retrieved successfully. Total: {Total}, Count: {Count}",
                    totalAmount, totalCount);

                return ResponseViewModel<PurchaseVolumeDto>.Success(result, "Purchase volume retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving purchase volume KPI for CompanyId: {CompanyId}", request.CompanyId);
                throw new BusinessLogicException(
                    "Failed to retrieve purchase volume data",
                    "Purchases",
                    Common.Enum.Finance_Enum.FinanceErrorCode.DatabaseError);
            }
        }
    }
}

