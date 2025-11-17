using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Finance.Infrastructure.Data;
using ModularERP.Modules.Inventory.Features.Warehouses.Models;
using ModularERP.Modules.Purchases.Goods_Receipt.Commends.Commends_GRNItem;
using ModularERP.Modules.Purchases.Goods_Receipt.DTO.DTO_GRN;
using ModularERP.Modules.Purchases.Goods_Receipt.Models;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Goods_Receipt.Handlers.Handlers_GRNItem
{
    public class AddGRNItemHandler : IRequestHandler<AddGRNItemCommand, ResponseViewModel<GRNLineItemResponseDto>>
    {
        private readonly IGeneralRepository<GoodsReceiptNote> _grnRepository;
        private readonly IGeneralRepository<GRNLineItem> _grnLineItemRepository;
        private readonly IGeneralRepository<POLineItem> _poLineItemRepository;
        private readonly IGeneralRepository<WarehouseStock> _warehouseStockRepository;
        private readonly FinanceDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly ILogger<AddGRNItemHandler> _logger;

        public AddGRNItemHandler(
            IGeneralRepository<GoodsReceiptNote> grnRepository,
            IGeneralRepository<GRNLineItem> grnLineItemRepository,
            IGeneralRepository<POLineItem> poLineItemRepository,
            IGeneralRepository<WarehouseStock> warehouseStockRepository,
            FinanceDbContext dbContext,
            IMapper mapper,
            ILogger<AddGRNItemHandler> logger)
        {
            _grnRepository = grnRepository;
            _grnLineItemRepository = grnLineItemRepository;
            _poLineItemRepository = poLineItemRepository;
            _warehouseStockRepository = warehouseStockRepository;
            _dbContext = dbContext;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<GRNLineItemResponseDto>> Handle(
            AddGRNItemCommand request,
            CancellationToken cancellationToken)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                _logger.LogInformation(
                    "🔵 Starting AddGRNItem | GRNId: {GRNId} | POLineItemId: {POLineItemId} | Qty: {Qty}",
                    request.GRNId, request.POLineItemId, request.ReceivedQuantity);

                // ✅ Step 1: Validate GRN exists and get WarehouseId
                var grnData = await _grnRepository.GetAll()
                    .Where(g => g.Id == request.GRNId)
                    .Select(g => new
                    {
                        g.Id,
                        g.GRNNumber,
                        g.WarehouseId,
                        g.PurchaseOrderId
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                if (grnData == null)
                {
                    throw new NotFoundException(
                        $"GRN with ID {request.GRNId} not found",
                        FinanceErrorCode.NotFound);
                }

                _logger.LogInformation(
                    "✅ GRN found | Number: {GRNNumber} | Warehouse: {WarehouseId}",
                    grnData.GRNNumber, grnData.WarehouseId);

                // ✅ Step 2: Get POLineItem data with Product info (using Projection)
                var poLineItemData = await _poLineItemRepository.GetAll()
                    .Where(x => x.Id == request.POLineItemId)
                    .Select(x => new
                    {
                        x.Id,
                        x.PurchaseOrderId,
                        x.ProductId,
                        x.ServiceId,
                        x.Quantity,
                        x.ReceivedQuantity,
                        x.RemainingQuantity,
                        x.UnitPrice,
                        x.Description,
                        ProductName = x.Product != null ? x.Product.Name : null,
                        ProductSKU = x.Product != null ? x.Product.SKU : null,
                        ServiceName = x.Service != null ? x.Service.Name : null
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                if (poLineItemData == null)
                {
                    throw new NotFoundException(
                        $"PO Line Item {request.POLineItemId} not found",
                        FinanceErrorCode.NotFound);
                }

                // Validate belongs to same PO
                if (poLineItemData.PurchaseOrderId != grnData.PurchaseOrderId)
                {
                    throw new BusinessLogicException(
                        $"Line item does not belong to the same Purchase Order",
                        "Purchases",
                        FinanceErrorCode.ValidationError);
                }

                string itemName = poLineItemData.ProductName
                    ?? poLineItemData.ServiceName
                    ?? poLineItemData.Description
                    ?? "Unknown Item";

                _logger.LogInformation(
                    "✅ POLineItem found | Item: '{ItemName}' | Ordered: {Ordered} | " +
                    "Received: {Received} | Remaining: {Remaining}",
                    itemName, poLineItemData.Quantity,
                    poLineItemData.ReceivedQuantity, poLineItemData.RemainingQuantity);

                // ✅ Step 3: Validate Quantity
                decimal pendingQty = poLineItemData.Quantity - poLineItemData.ReceivedQuantity;

                if (request.ReceivedQuantity <= 0)
                {
                    throw new BusinessLogicException(
                        $"Received quantity must be greater than zero",
                        "Purchases",
                        FinanceErrorCode.ValidationError);
                }

                if (pendingQty <= 0)
                {
                    throw new BusinessLogicException(
                        $"Cannot receive '{itemName}'. Already fully received",
                        "Purchases",
                        FinanceErrorCode.ValidationError);
                }

                if (request.ReceivedQuantity > pendingQty)
                {
                    throw new BusinessLogicException(
                        $"Cannot receive {request.ReceivedQuantity} units of '{itemName}'. " +
                        $"Only {pendingQty} units pending",
                        "Purchases",
                        FinanceErrorCode.ValidationError);
                }

                // ✅ Step 4: Create GRN Line Item
                var lineItem = new GRNLineItem
                {
                    Id = Guid.NewGuid(),
                    GRNId = request.GRNId,
                    POLineItemId = request.POLineItemId,
                    ReceivedQuantity = request.ReceivedQuantity,
                    Notes = request.Notes,
                    CreatedById = request.UserId,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    IsDeleted = false
                };

                await _grnLineItemRepository.AddAsync(lineItem);
                await _grnLineItemRepository.SaveChanges();

                _logger.LogInformation(
                    "✅ GRN Line Item created | Id: {LineItemId}",
                    lineItem.Id);

                // ✅ Step 5: Update POLineItem (ReceivedQuantity & RemainingQuantity)
                var poLineItem = await _poLineItemRepository.GetByIDWithTracking(request.POLineItemId);
                if (poLineItem != null)
                {
                    decimal oldReceived = poLineItem.ReceivedQuantity;
                    decimal oldRemaining = poLineItem.RemainingQuantity;

                    poLineItem.ReceivedQuantity += request.ReceivedQuantity;
                    poLineItem.RemainingQuantity = poLineItem.Quantity - poLineItem.ReceivedQuantity;
                    poLineItem.UpdatedAt = DateTime.UtcNow;
                    poLineItem.UpdatedById = request.UserId;

                    await _poLineItemRepository.SaveChanges();

                    _logger.LogInformation(
                        "✅ POLineItem updated | Id: {POLineItemId} | " +
                        "Received: {OldReceived} → {NewReceived} | " +
                        "Remaining: {OldRemaining} → {NewRemaining}",
                        request.POLineItemId,
                        oldReceived, poLineItem.ReceivedQuantity,
                        oldRemaining, poLineItem.RemainingQuantity);
                }

                // ✅ Step 6: Update Warehouse Stock (Products only, skip Services)
                if (poLineItemData.ProductId.HasValue)
                {
                    await UpdateWarehouseStock(
                        grnData.WarehouseId,
                        poLineItemData.ProductId.Value,
                        request.ReceivedQuantity,
                        poLineItemData.UnitPrice,
                        itemName,
                        //request.UserId,
                        cancellationToken);
                }
                else
                {
                    _logger.LogInformation(
                        "⏭️ Skipping stock update for Service: '{ItemName}'",
                        itemName);
                }

                // ✅ Step 7: Commit Transaction
                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation(
                    "✅✅✅ GRN Item added successfully | LineItemId: {LineItemId}",
                    lineItem.Id);

                // ✅ Step 8: Build Response using Projection
                var result = await _grnLineItemRepository.GetAll()
                    .Where(l => l.Id == lineItem.Id)
                    .Select(l => new GRNLineItemResponseDto
                    {
                        Id = l.Id,
                        GRNId = l.GRNId,
                        POLineItemId = l.POLineItemId,
                        ProductId = l.POLineItem.ProductId,
                        ProductName = l.POLineItem.Product != null
                            ? l.POLineItem.Product.Name
                            : (l.POLineItem.Service != null
                                ? l.POLineItem.Service.Name
                                : l.POLineItem.Description),
                        ProductSKU = l.POLineItem.Product != null
                            ? l.POLineItem.Product.SKU
                            : null,
                        OrderedQuantity = l.POLineItem.Quantity,
                        PreviouslyReceivedQuantity = l.POLineItem.ReceivedQuantity - l.ReceivedQuantity,
                        ReceivedQuantity = l.ReceivedQuantity,
                        RemainingQuantity = l.POLineItem.RemainingQuantity,
                        IsFullyReceived = l.POLineItem.RemainingQuantity == 0,
                        UnitOfMeasure = null,
                        Notes = l.Notes
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                return ResponseViewModel<GRNLineItemResponseDto>.Success(
                    result!,
                    "GRN Item added successfully and stock updated");
            }
            catch (Exception ex) when (ex is not BaseApplicationException)
            {
                await transaction.RollbackAsync(cancellationToken);

                _logger.LogError(ex,
                    "❌ Error adding item to GRN | GRNId: {GRNId} | POLineItemId: {POLineItemId}",
                    request.GRNId, request.POLineItemId);

                throw new BusinessLogicException(
                    $"Error adding GRN item: {ex.Message}",
                    "Purchases",
                    FinanceErrorCode.BusinessLogicError);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        private async Task UpdateWarehouseStock(
            Guid warehouseId,
            Guid productId,
            decimal quantityToAdd,
            decimal unitPrice,
            string productName,
           // Guid? userId,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "🔄 Updating stock | Warehouse: {WarehouseId} | Product: {ProductId} | Qty: {Qty}",
                warehouseId, productId, quantityToAdd);

            var existingStock = await _warehouseStockRepository.GetAll()
                .Where(x => x.WarehouseId == warehouseId && x.ProductId == productId)
                .FirstOrDefaultAsync(cancellationToken);

            if (existingStock != null)
            {
                // Update existing stock
                decimal oldQty = existingStock.Quantity;
                decimal oldAvailable = existingStock.AvailableQuantity;

                existingStock.Quantity += quantityToAdd;
                existingStock.AvailableQuantity += quantityToAdd;
                existingStock.LastStockInDate = DateTime.UtcNow;
                existingStock.UpdatedAt = DateTime.UtcNow;
                existingStock.UpdatedById = Guid.Parse("f0602c31-0c12-4b5c-9ccf-fe17811d5c53");//userId;
                existingStock.TotalValue = existingStock.Quantity * (existingStock.AverageUnitCost ?? unitPrice);

                await _warehouseStockRepository.SaveChanges();

                _logger.LogInformation(
                    "✅ Stock updated | Product: '{ProductName}' | " +
                    "Qty: {OldQty} → {NewQty} | Available: {OldAvailable} → {NewAvailable}",
                    productName, oldQty, existingStock.Quantity,
                    oldAvailable, existingStock.AvailableQuantity);
            }
            else
            {
                // Create new stock record
                var newStock = new WarehouseStock
                {
                    WarehouseId = warehouseId,
                    ProductId = productId,
                    Quantity = quantityToAdd,
                    AvailableQuantity = quantityToAdd,
                    ReservedQuantity = 0,
                    AverageUnitCost = unitPrice,
                    TotalValue = unitPrice * quantityToAdd,
                    LastStockInDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = Guid.Parse("f0602c31-0c12-4b5c-9ccf-fe17811d5c53"),//userId,
                    IsActive = true,
                    IsDeleted = false
                };

                await _warehouseStockRepository.AddAsync(newStock);
                await _warehouseStockRepository.SaveChanges();

                _logger.LogInformation(
                    "✅ New stock created | Product: '{ProductName}' | Initial Qty: {Qty}",
                    productName, quantityToAdd);
            }
        }
    }
}