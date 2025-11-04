using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Goods_Receipt.DTO.DTO_GRN;
using ModularERP.Modules.Purchases.Goods_Receipt.Models;
using ModularERP.Modules.Purchases.Goods_Receipt.Qeuries.Qeuries_GRN;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Goods_Receipt.Handlers.Handlers_GRN
{
    public class GetAllGRNsHandler : IRequestHandler<GetAllGRNsQuery, ResponseViewModel<List<GRNListItemDto>>>
    {
        private readonly IGeneralRepository<GoodsReceiptNote> _grnRepository;
        private readonly ILogger<GetAllGRNsHandler> _logger;

        public GetAllGRNsHandler(
            IGeneralRepository<GoodsReceiptNote> grnRepository,
            ILogger<GetAllGRNsHandler> logger)
        {
            _grnRepository = grnRepository;
            _logger = logger;
        }

        public async Task<ResponseViewModel<List<GRNListItemDto>>> Handle(GetAllGRNsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Fetching all GRNs for Company: {CompanyId}", request.CompanyId);

                var query = _grnRepository.GetByCompanyId(request.CompanyId);

                // Apply filters
                if (request.FromDate.HasValue)
                {
                    query = query.Where(x => x.ReceiptDate >= request.FromDate.Value);
                }

                if (request.ToDate.HasValue)
                {
                    query = query.Where(x => x.ReceiptDate <= request.ToDate.Value);
                }

                if (request.WarehouseId.HasValue)
                {
                    query = query.Where(x => x.WarehouseId == request.WarehouseId.Value);
                }

                if (request.PurchaseOrderId.HasValue)
                {
                    query = query.Where(x => x.PurchaseOrderId == request.PurchaseOrderId.Value);
                }

                // Project to DTO using Select (projection instead of Include)
                var grns = await query
                    .OrderByDescending(x => x.CreatedAt)
                    .Select(g => new GRNListItemDto
                    {
                        Id = g.Id,
                        GRNNumber = g.GRNNumber,
                        PurchaseOrderNumber = g.PurchaseOrder.PONumber,
                        WarehouseName = g.Warehouse.Name,
                        ReceiptDate = g.ReceiptDate,
                        ReceivedBy = g.ReceivedBy,
                        LineItemsCount = g.LineItems.Count,
                        CreatedDate = g.CreatedAt
                    })
                    .ToListAsync(cancellationToken);

                _logger.LogInformation("Retrieved {Count} GRNs for Company: {CompanyId}", grns.Count, request.CompanyId);

                return ResponseViewModel<List<GRNListItemDto>>.Success(
                    grns,
                    $"Retrieved {grns.Count} GRN(s) successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching GRNs for Company: {CompanyId}", request.CompanyId);
                throw;
            }
        }
    }
}
