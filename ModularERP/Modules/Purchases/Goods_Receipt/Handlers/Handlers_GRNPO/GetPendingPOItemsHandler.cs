using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Modules.Purchases.Goods_Receipt.DTO.DTO_GRN;
using ModularERP.Modules.Purchases.Goods_Receipt.Models;
using ModularERP.Modules.Purchases.Goods_Receipt.Qeuries.Qeuires_GRNPO;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Goods_Receipt.Handlers
{
    public class GetPendingPOItemsHandler : IRequestHandler<GetPendingPOItemsQuery, List<PendingPOItemDto>>
    {
        private readonly IGeneralRepository<POLineItem> _poLineItemRepository;
        private readonly IGeneralRepository<GRNLineItem> _grnLineItemRepository;
        private readonly ILogger<GetPendingPOItemsHandler> _logger;

        public GetPendingPOItemsHandler(
            IGeneralRepository<POLineItem> poLineItemRepository,
            IGeneralRepository<GRNLineItem> grnLineItemRepository,
            ILogger<GetPendingPOItemsHandler> logger)
        {
            _poLineItemRepository = poLineItemRepository;
            _grnLineItemRepository = grnLineItemRepository;
            _logger = logger;
        }

        public async Task<List<PendingPOItemDto>> Handle(GetPendingPOItemsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Getting pending items for PO: {PurchaseOrderId}", request.PurchaseOrderId);

                var poLineItems = await _poLineItemRepository
                    .GetAll()
                    .Where(p => p.PurchaseOrderId == request.PurchaseOrderId)
                    .Select(p => new
                    {
                        p.Id,
                        p.ProductId,
                        ProductName = p.Product.Name,
                        ProductSKU = p.Product.SKU ?? string.Empty,
                        p.Quantity
                    })
                    .ToListAsync(cancellationToken);

                var poLineItemIds = poLineItems.Select(p => p.Id).ToList();

                var receivedQuantities = await _grnLineItemRepository
                    .GetAll()
                    .Where(g => poLineItemIds.Contains(g.POLineItemId))
                    .GroupBy(g => g.POLineItemId)
                    .Select(g => new
                    {
                        POLineItemId = g.Key,
                        TotalReceived = g.Sum(x => x.ReceivedQuantity)
                    })
                    .ToListAsync(cancellationToken);

                var receivedDict = receivedQuantities.ToDictionary(r => r.POLineItemId, r => r.TotalReceived);

                var pendingItems = poLineItems
                    .Select(item => new PendingPOItemDto
                    {
                        POLineItemId = item.Id,
                        ProductId = item.ProductId,
                        ProductName = item.ProductName,
                        ProductSKU = item.ProductSKU,
                        OrderedQuantity = item.Quantity,
                        ReceivedQuantity = receivedDict.ContainsKey(item.Id) ? receivedDict[item.Id] : 0,
                        PendingQuantity = item.Quantity - (receivedDict.ContainsKey(item.Id) ? receivedDict[item.Id] : 0),
                        UnitOfMeasure = null
                    })
                    .Where(p => p.PendingQuantity > 0)
                    .ToList();

                _logger.LogInformation("Found {Count} pending items for PO: {PurchaseOrderId}",
                    pendingItems.Count, request.PurchaseOrderId);

                return pendingItems;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending items for PO: {PurchaseOrderId}", request.PurchaseOrderId);
                throw new BusinessLogicException(
                    $"Error retrieving pending items: {ex.Message}",
                    "Purchases",
                    FinanceErrorCode.BusinessLogicError);
            }
        }
    }
}