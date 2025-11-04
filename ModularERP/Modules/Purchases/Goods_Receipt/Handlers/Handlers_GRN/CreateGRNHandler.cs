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
    public class CreateGRNHandler : IRequestHandler<CreateGRNCommand, ResponseViewModel<GRNResponseDto>>
    {
        private readonly IGeneralRepository<GoodsReceiptNote> _grnRepository;
        private readonly IGeneralRepository<GRNLineItem> _lineItemRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateGRNHandler> _logger;

        public CreateGRNHandler(
            IGeneralRepository<GoodsReceiptNote> grnRepository,
            IGeneralRepository<GRNLineItem> lineItemRepository,
            IMapper mapper,
           ILogger<CreateGRNHandler> logger)
        {
            _grnRepository = grnRepository;
            _lineItemRepository = lineItemRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<GRNResponseDto>> Handle(CreateGRNCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Creating new GRN for PO: {PurchaseOrderId}", request.Data.PurchaseOrderId);

                // Validate Purchase Order exists
                var poExists = await _grnRepository.GetAll()
                    .AnyAsync(x => x.PurchaseOrderId == request.Data.PurchaseOrderId, cancellationToken);

                if (!poExists)
                {
                    // Check if PO exists in PO table (you might need separate repository)
                    throw new NotFoundException(
                        $"Purchase Order with ID {request.Data.PurchaseOrderId} not found",
                        FinanceErrorCode.NotFound);
                }

                // Generate GRN Number
                var grnNumber = await GenerateGRNNumber(request.Data.CompanyId, cancellationToken);

                // Map to entity
                var grn = _mapper.Map<GoodsReceiptNote>(request.Data);
                grn.GRNNumber = grnNumber;

                // Add GRN
                await _grnRepository.AddAsync(grn);
                await _grnRepository.SaveChanges();

                _logger.LogInformation("GRN created successfully with number: {GRNNumber}", grnNumber);

                // Get created GRN with related data using projection
                var createdGrn = await _grnRepository.GetAll()
                    .Where(x => x.Id == grn.Id)
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

                if (createdGrn == null)
                {
                    throw new NotFoundException("GRN not found after creation", FinanceErrorCode.NotFound);
                }

                return ResponseViewModel<GRNResponseDto>.Success(
                    createdGrn,
                    "GRN created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating GRN for PO: {PurchaseOrderId}", request.Data.PurchaseOrderId);
                throw;
            }
        }

        private async Task<string> GenerateGRNNumber(Guid companyId, CancellationToken cancellationToken)
        {
            var year = DateTime.UtcNow.Year;
            var lastGrn = await _grnRepository.GetByCompanyId(companyId)
                .Where(x => x.GRNNumber.StartsWith($"GRN-{year}-"))
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => x.GRNNumber)
                .FirstOrDefaultAsync(cancellationToken);

            int nextNumber = 1;
            if (!string.IsNullOrEmpty(lastGrn))
            {
                var parts = lastGrn.Split('-');
                if (parts.Length == 3 && int.TryParse(parts[2], out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"GRN-{year}-{nextNumber:D5}";
        }
    }
}
