using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Models;
using ModularERP.Modules.Purchases.Refunds.DTO.DTO_RefundInvoce;
using ModularERP.Modules.Purchases.Refunds.Models;
using ModularERP.Modules.Purchases.Refunds.Qeuries.Qeuries_RefundInvoce;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Refunds.Handlers.Handlers_Refund
{
    public class GetAllRefundsHandler : IRequestHandler<GetAllRefundsQuery, ResponseViewModel<List<RefundDto>>>
    {
        private readonly IGeneralRepository<PurchaseRefund> _refundRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<GetAllRefundsHandler> _logger;

        public GetAllRefundsHandler(
            IGeneralRepository<PurchaseRefund> refundRepo,
            IMapper mapper,
            ILogger<GetAllRefundsHandler> logger)
        {
            _refundRepo = refundRepo;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<List<RefundDto>>> Handle(GetAllRefundsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Retrieving all refunds with filters - Supplier: {SupplierId}, From: {FromDate}, To: {ToDate}, Page: {PageNumber}",
                    request.SupplierId, request.FromDate, request.ToDate, request.PageNumber);

                // Build query with all necessary includes
                var query = _refundRepo.GetAll()
                    .Include(r => r.PurchaseOrder)
                    .Include(r => r.Supplier)
                    .Include(r => r.DebitNote)
                    .Include(r => r.LineItems)
                        .ThenInclude(rl => rl.GRNLineItem)
                        .ThenInclude(g => g.POLineItem)
                        .ThenInclude(pol => pol.Product)
                    .AsQueryable();

                // Apply filters
                if (request.SupplierId.HasValue)
                {
                    query = query.Where(r => r.SupplierId == request.SupplierId.Value);
                    _logger.LogInformation("Filtering by SupplierId: {SupplierId}", request.SupplierId.Value);
                }

                if (request.FromDate.HasValue)
                {
                    query = query.Where(r => r.RefundDate >= request.FromDate.Value);
                    _logger.LogInformation("Filtering from date: {FromDate}", request.FromDate.Value);
                }

                if (request.ToDate.HasValue)
                {
                    query = query.Where(r => r.RefundDate <= request.ToDate.Value);
                    _logger.LogInformation("Filtering to date: {ToDate}", request.ToDate.Value);
                }

                // Get total count before pagination
                var totalCount = await query.CountAsync(cancellationToken);

                // Apply pagination
                var skip = (request.PageNumber - 1) * request.PageSize;
                var refunds = await query
                    .OrderByDescending(r => r.CreatedAt)
                    .Skip(skip)
                    .Take(request.PageSize)
                    .ToListAsync(cancellationToken);

                _logger.LogInformation("Retrieved {Count} refunds out of {TotalCount} total", refunds.Count, totalCount);

                // Map to DTOs
                var refundDtos = refunds.Select(refund => new RefundDto
                {
                    Id = refund.Id,
                    RefundNumber = refund.RefundNumber,
                    PurchaseOrderId = refund.PurchaseOrderId,
                    PurchaseOrderNumber = refund.PurchaseOrder?.PONumber,
                    SupplierId = refund.SupplierId,
                    SupplierName = refund.Supplier?.Name,
                    RefundDate = refund.RefundDate,
                    TotalAmount = refund.TotalAmount,
                    Reason = refund.Reason,
                    Notes = refund.Notes,
                    HasDebitNote = refund.DebitNote != null,
                    DebitNote = refund.DebitNote != null ? new DebitNoteDto
                    {
                        Id = refund.DebitNote.Id,
                        DebitNoteNumber = refund.DebitNote.DebitNoteNumber,
                        RefundId = refund.DebitNote.RefundId,
                        SupplierId = refund.DebitNote.SupplierId,
                        SupplierName = refund.Supplier?.Name ?? "",
                        NoteDate = refund.DebitNote.NoteDate,
                        Amount = refund.DebitNote.Amount,
                        Notes = refund.DebitNote.Notes
                    } : null,
                    LineItems = refund.LineItems.Select(rl => new RefundLineItemDto
                    {
                        Id = rl.Id,
                        RefundId = rl.RefundId,
                        GRNLineItemId = rl.GRNLineItemId,
                        ProductId = rl.GRNLineItem?.POLineItem?.ProductId ?? Guid.Empty,
                        ProductName = rl.GRNLineItem?.POLineItem?.Product?.Name ?? "",
                        ProductSKU = rl.GRNLineItem?.POLineItem?.Product?.SKU ?? "",
                        ReturnQuantity = rl.ReturnQuantity,
                        UnitPrice = rl.UnitPrice,
                        LineTotal = rl.LineTotal
                    }).ToList(),
                    // Note: StatusUpdates and InventoryImpact are not included in list view for performance
                    // They can be fetched when viewing individual refund details
                    StatusUpdates = null,
                    InventoryImpact = null,
                    CreatedAt = refund.CreatedAt
                }).ToList();

                var response = new
                {
                    Data = refundDtos,
                    Pagination = new
                    {
                        TotalCount = totalCount,
                        PageNumber = request.PageNumber,
                        PageSize = request.PageSize,
                        TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize),
                        HasPreviousPage = request.PageNumber > 1,
                        HasNextPage = skip + request.PageSize < totalCount
                    }
                };

                _logger.LogInformation("Successfully retrieved refunds with pagination info");

                return ResponseViewModel<List<RefundDto>>.Success(
                    refundDtos,
                    $"Retrieved {refundDtos.Count} refunds (Page {request.PageNumber} of {response.Pagination.TotalPages})");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving refunds");
                throw;
            }
        }
    }

    public class GetRefundByIdHandler : IRequestHandler<GetRefundByIdQuery, ResponseViewModel<RefundDto>>
    {
        private readonly IGeneralRepository<PurchaseRefund> _refundRepo;
        private readonly IGeneralRepository<PurchaseOrder> _poRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<GetRefundByIdHandler> _logger;

        public GetRefundByIdHandler(
            IGeneralRepository<PurchaseRefund> refundRepo,
            IGeneralRepository<PurchaseOrder> poRepo,
            IMapper mapper,
            ILogger<GetRefundByIdHandler> logger)
        {
            _refundRepo = refundRepo;
            _poRepo = poRepo;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<RefundDto>> Handle(GetRefundByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Retrieving refund details: {RefundId}", request.Id);

                // Load refund with all related data
                var refund = await _refundRepo.GetAll()
                    .Include(r => r.PurchaseOrder)
                        .ThenInclude(po => po.LineItems)
                    .Include(r => r.Supplier)
                    .Include(r => r.DebitNote)
                    .Include(r => r.LineItems)
                        .ThenInclude(rl => rl.GRNLineItem)
                        .ThenInclude(g => g.POLineItem)
                        .ThenInclude(pol => pol.Product)
                    .Include(r => r.LineItems)
                        .ThenInclude(rl => rl.GRNLineItem)
                        .ThenInclude(g => g.GRN)
                    .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

                if (refund == null)
                {
                    _logger.LogWarning("Refund not found: {RefundId}", request.Id);
                    throw new NotFoundException(
                        $"Refund with ID {request.Id} not found",
                        FinanceErrorCode.NotFound);
                }

                _logger.LogInformation("Found refund: {RefundNumber}, preparing detailed response", refund.RefundNumber);

                // Build inventory impact list
                var inventoryImpacts = new List<InventoryImpactDto>();
                foreach (var lineItem in refund.LineItems)
                {
                    if (lineItem.GRNLineItem?.POLineItem?.ProductId != null)
                    {
                        inventoryImpacts.Add(new InventoryImpactDto
                        {
                            ProductId = lineItem.GRNLineItem.POLineItem.ProductId.Value,
                            ProductName = lineItem.GRNLineItem.POLineItem.Product?.Name ?? "",
                            ProductSKU = lineItem.GRNLineItem.POLineItem.Product?.SKU ?? "",
                            WarehouseId = lineItem.GRNLineItem.GRN?.WarehouseId ?? Guid.Empty,
                            QuantityReturned = lineItem.ReturnQuantity,
                            NewStockLevel = 0 // Will be calculated from WarehouseStock if needed
                        });
                    }
                }

                // Build complete DTO with all details
                var refundDto = new RefundDto
                {
                    Id = refund.Id,
                    RefundNumber = refund.RefundNumber,
                    PurchaseOrderId = refund.PurchaseOrderId,
                    PurchaseOrderNumber = refund.PurchaseOrder?.PONumber,
                    SupplierId = refund.SupplierId,
                    SupplierName = refund.Supplier?.Name,
                    RefundDate = refund.RefundDate,
                    TotalAmount = refund.TotalAmount,
                    Reason = refund.Reason,
                    Notes = refund.Notes,
                    HasDebitNote = refund.DebitNote != null,
                    DebitNote = refund.DebitNote != null ? new DebitNoteDto
                    {
                        Id = refund.DebitNote.Id,
                        DebitNoteNumber = refund.DebitNote.DebitNoteNumber,
                        RefundId = refund.DebitNote.RefundId,
                        SupplierId = refund.DebitNote.SupplierId,
                        SupplierName = refund.Supplier?.Name ?? "",
                        NoteDate = refund.DebitNote.NoteDate,
                        Amount = refund.DebitNote.Amount,
                        Notes = refund.DebitNote.Notes
                    } : null,
                    LineItems = refund.LineItems.Select(rl => new RefundLineItemDto
                    {
                        Id = rl.Id,
                        RefundId = rl.RefundId,
                        GRNLineItemId = rl.GRNLineItemId,
                        ProductId = rl.GRNLineItem?.POLineItem?.ProductId ?? Guid.Empty,
                        ProductName = rl.GRNLineItem?.POLineItem?.Product?.Name ?? "",
                        ProductSKU = rl.GRNLineItem?.POLineItem?.Product?.SKU ?? "",
                        ReturnQuantity = rl.ReturnQuantity,
                        UnitPrice = rl.UnitPrice,
                        LineTotal = rl.LineTotal
                    }).ToList(),
                    StatusUpdates = new StatusUpdatesDto
                    {
                        PurchaseOrder = new POStatusDto
                        {
                            ReceptionStatus = refund.PurchaseOrder?.ReceptionStatus ?? "Unknown",
                            PaymentStatus = refund.PurchaseOrder?.PaymentStatus ?? "Unknown",
                            DocumentStatus = refund.PurchaseOrder?.DocumentStatus ?? "Unknown",
                            TotalOrdered = refund.PurchaseOrder?.LineItems.Sum(l => l.Quantity) ?? 0,
                            TotalReceived = refund.PurchaseOrder?.LineItems.Sum(l => l.ReceivedQuantity) ?? 0,
                            TotalReturned = refund.PurchaseOrder?.LineItems.Sum(l => l.ReturnedQuantity) ?? 0,
                            NetReceived = refund.PurchaseOrder?.LineItems.Sum(l => l.ReceivedQuantity - l.ReturnedQuantity) ?? 0
                        },
                        Supplier = new SupplierStatusDto
                        {
                            SupplierId = refund.SupplierId,
                            SupplierName = refund.Supplier?.Name ?? "",
                            BalanceAdjusted = refund.TotalAmount,
                            NewBalance = 0 // Calculated from AP system
                        }
                    },
                    InventoryImpact = inventoryImpacts,
                    CreatedAt = refund.CreatedAt
                };

                _logger.LogInformation("Successfully retrieved refund details: {RefundNumber} with {LineItemCount} line items",
                    refund.RefundNumber, refund.LineItems.Count);

                return ResponseViewModel<RefundDto>.Success(refundDto, "Refund retrieved successfully");
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving refund: {RefundId}", request.Id);
                throw;
            }
        }
    }
}