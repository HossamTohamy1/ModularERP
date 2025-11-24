using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Refunds.DTO.DTO_RefundInvoce;
using ModularERP.Modules.Purchases.Refunds.Models;
using ModularERP.Modules.Purchases.Refunds.Qeuries.Qeuries_RefundInvoce;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Refunds.Handlers.Handlers_RefundInvoce
{
    public class GetRefundsByPOHandler : IRequestHandler<GetRefundsByPOQuery, ResponseViewModel<List<RefundDto>>>
    {
        private readonly IGeneralRepository<PurchaseRefund> _refundRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<GetRefundsByPOHandler> _logger;

        public GetRefundsByPOHandler(
            IGeneralRepository<PurchaseRefund> refundRepo,
            IMapper mapper,
            ILogger<GetRefundsByPOHandler> logger)
        {
            _refundRepo = refundRepo;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<List<RefundDto>>> Handle(
            GetRefundsByPOQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Retrieving refunds for PO: {PurchaseOrderId}", request.PurchaseOrderId);

                // نفس الـ projection المستخدم في CreateRefundFromPOHandler
                var refunds = await _refundRepo.GetAll()
                    .Where(r => r.PurchaseOrderId == request.PurchaseOrderId)
                    .Include(r => r.PurchaseOrder)
                    .Include(r => r.Supplier)
                    .Include(r => r.DebitNote)
                        .ThenInclude(dn => dn.Supplier)
                    .Include(r => r.LineItems)
                        .ThenInclude(rl => rl.GRNLineItem)
                            .ThenInclude(grn => grn.POLineItem)
                                .ThenInclude(pol => pol.Product)
                    .OrderByDescending(r => r.CreatedAt)
                    .Select(r => new RefundDto
                    {
                        Id = r.Id,
                        RefundNumber = r.RefundNumber,
                        PurchaseOrderId = r.PurchaseOrderId,
                        PurchaseOrderNumber = r.PurchaseOrder.PONumber,
                        SupplierId = r.SupplierId,
                        SupplierName = r.Supplier.Name,
                        RefundDate = r.RefundDate,
                        TotalAmount = r.TotalAmount,
                        Reason = r.Reason,
                        Notes = r.Notes,
                        HasDebitNote = r.DebitNote != null,

                        // Debit Note Details (if exists)
                        DebitNote = r.DebitNote != null ? new DebitNoteDto
                        {
                            Id = r.DebitNote.Id,
                            DebitNoteNumber = r.DebitNote.DebitNoteNumber,
                            RefundId = r.DebitNote.RefundId,
                            SupplierId = r.DebitNote.SupplierId,
                            SupplierName = r.DebitNote.Supplier.Name,
                            Amount = r.DebitNote.Amount,
                            NoteDate = r.DebitNote.NoteDate,
                            Notes = r.DebitNote.Notes
                        } : null,

                        // Line Items with full product details
                        LineItems = r.LineItems.Select(rl => new RefundLineItemDto
                        {
                            Id = rl.Id,
                            RefundId = rl.RefundId,
                            GRNLineItemId = rl.GRNLineItemId,
                            ProductId = (Guid)rl.GRNLineItem.POLineItem.ProductId,
                            ProductName = rl.GRNLineItem.POLineItem.Product.Name.Trim(),
                            ProductSKU = rl.GRNLineItem.POLineItem.Product.SKU ?? "N/A",
                            ReturnQuantity = rl.ReturnQuantity,
                            UnitPrice = rl.UnitPrice,
                            LineTotal = rl.LineTotal
                        }).ToList(),

                
                        StatusUpdates = null,
                        InventoryImpact = null,

                        CreatedAt = r.CreatedAt
                    })
                    .ToListAsync(cancellationToken);

                _logger.LogInformation("Retrieved {Count} refunds for PO: {PurchaseOrderId}",
                    refunds.Count, request.PurchaseOrderId);

                return ResponseViewModel<List<RefundDto>>.Success(
                    refunds,
                    $"Retrieved {refunds.Count} refund(s) successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving refunds for PO: {PurchaseOrderId}", request.PurchaseOrderId);
                throw;
            }
        }
    }
}