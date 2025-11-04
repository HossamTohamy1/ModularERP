using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Goods_Receipt.Commends.Commends_GRN;
using ModularERP.Modules.Purchases.Goods_Receipt.DTO.DTO_GRN;
using ModularERP.Modules.Purchases.Goods_Receipt.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Goods_Receipt.Handlers.Handlers_GRN
{
    public class UpdateGRNHandler : IRequestHandler<UpdateGRNCommand, ResponseViewModel<GRNResponseDto>>
    {
        private readonly IGeneralRepository<GoodsReceiptNote> _grnRepository;
        private readonly IGeneralRepository<GRNLineItem> _lineItemRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<UpdateGRNHandler> _logger;

        public UpdateGRNHandler(
            IGeneralRepository<GoodsReceiptNote> grnRepository,
            IGeneralRepository<GRNLineItem> lineItemRepository,
            IMapper mapper,
            ILogger<UpdateGRNHandler> logger)
        {
            _grnRepository = grnRepository;
            _lineItemRepository = lineItemRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<GRNResponseDto>> Handle(UpdateGRNCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Updating GRN with ID: {GRNId}", request.Data.Id);

                // Get existing GRN
                var existingGrn = await _grnRepository.GetByIDWithTracking(request.Data.Id);
                if (existingGrn == null)
                {
                    throw new NotFoundException(
                        $"GRN with ID {request.Data.Id} not found",
                        FinanceErrorCode.NotFound);
                }

                // Update main properties
                existingGrn.PurchaseOrderId = request.Data.PurchaseOrderId;
                existingGrn.WarehouseId = request.Data.WarehouseId;
                existingGrn.ReceiptDate = request.Data.ReceiptDate;
                existingGrn.ReceivedBy = request.Data.ReceivedBy;
                existingGrn.Notes = request.Data.Notes;
                existingGrn.UpdatedAt = DateTime.UtcNow;

                // Get existing line items
                var existingLineItems = await _lineItemRepository.GetAll()
                    .Where(x => x.GRNId == request.Data.Id)
                    .ToListAsync(cancellationToken);

                // Determine line items to delete
                var lineItemIdsToKeep = request.Data.LineItems
                    .Where(x => x.Id.HasValue)
                    .Select(x => x.Id!.Value)
                    .ToList();

                var lineItemsToDelete = existingLineItems
                    .Where(x => !lineItemIdsToKeep.Contains(x.Id))
                    .ToList();

                // Delete removed line items
                foreach (var lineItem in lineItemsToDelete)
                {
                    await _lineItemRepository.Delete(lineItem.Id);
                }

                // Update or add line items
                foreach (var lineItemDto in request.Data.LineItems)
                {
                    if (lineItemDto.Id.HasValue)
                    {
                        // Update existing line item
                        var existingLineItem = existingLineItems.FirstOrDefault(x => x.Id == lineItemDto.Id.Value);
                        if (existingLineItem != null)
                        {
                            existingLineItem.POLineItemId = lineItemDto.POLineItemId;
                            existingLineItem.ReceivedQuantity = lineItemDto.ReceivedQuantity;
                            existingLineItem.Notes = lineItemDto.Notes;
                            existingLineItem.UpdatedAt = DateTime.UtcNow;
                        }
                    }
                    else
                    {
                        // Add new line item
                        var newLineItem = new GRNLineItem
                        {
                            GRNId = request.Data.Id,
                            POLineItemId = lineItemDto.POLineItemId,
                            ReceivedQuantity = lineItemDto.ReceivedQuantity,
                            Notes = lineItemDto.Notes
                        };
                        await _lineItemRepository.AddAsync(newLineItem);
                    }
                }

                await _grnRepository.SaveChanges();

                _logger.LogInformation("GRN updated successfully: {GRNId}", request.Data.Id);

                // Get updated GRN with projections
                var updatedGrn = await _grnRepository.GetAll()
                    .Where(x => x.Id == request.Data.Id)
                    .Select(g => new GRNResponseDto
                    {
                        Id = g.Id,
                        GRNNumber = g.GRNNumber,
                        PurchaseOrderId = g.PurchaseOrderId,
                        PurchaseOrderNumber = g.PurchaseOrder.PONumber,
                        WarehouseId = g.WarehouseId,
                        WarehouseName = g.Warehouse.Name,
                        ReceiptDate = g.ReceiptDate,
                        ReceivedBy = g.ReceivedBy,
                        Notes = g.Notes,
                        CompanyId = g.CompanyId,
                        CreatedAt = g.CreatedAt,
                        CreatedById = g.CreatedById,
                        LineItems = g.LineItems.Select(li => new GRNLineItemResponseDto
                        {
                            Id = li.Id,
                            POLineItemId = li.POLineItemId,
                            ProductName = li.POLineItem.Product.Name,
                            ProductSKU = li.POLineItem.Product.SKU,
                            ReceivedQuantity = li.ReceivedQuantity,
                            Notes = li.Notes
                        }).ToList()
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                if (updatedGrn == null)
                {
                    throw new NotFoundException("GRN not found after update", FinanceErrorCode.NotFound);
                }

                return ResponseViewModel<GRNResponseDto>.Success(
                    updatedGrn,
                    "GRN updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating GRN with ID: {GRNId}", request.Data.Id);
                throw;
            }
        }
    }
}
