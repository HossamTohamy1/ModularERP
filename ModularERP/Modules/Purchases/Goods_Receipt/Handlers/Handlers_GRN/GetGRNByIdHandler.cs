using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Goods_Receipt.DTO.DTO_GRN;
using ModularERP.Modules.Purchases.Goods_Receipt.Models;
using ModularERP.Modules.Purchases.Goods_Receipt.Qeuries.Qeuries_GRN;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Goods_Receipt.Handlers.Handlers_GRN
{
    public class GetGRNByIdHandler : IRequestHandler<GetGRNByIdQuery, ResponseViewModel<GRNResponseDto>>
    {
        private readonly IGeneralRepository<GoodsReceiptNote> _grnRepository;
        private readonly ILogger<GetGRNByIdHandler> _logger;

        public GetGRNByIdHandler(
            IGeneralRepository<GoodsReceiptNote> grnRepository,
            ILogger<GetGRNByIdHandler> logger)
        {
            _grnRepository = grnRepository;
            _logger = logger;
        }

        public async Task<ResponseViewModel<GRNResponseDto>> Handle(GetGRNByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Fetching GRN with ID: {GRNId}", request.Id);

                // Use projection to get data without Include
                var grn = await _grnRepository.GetAll()
                    .Where(x => x.Id == request.Id)
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
                        LineItems = g.LineItems
                            .Where(li => !li.IsDeleted)
                            .Select(li => new GRNLineItemResponseDto
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

                if (grn == null)
                {
                    throw new NotFoundException(
                        $"GRN with ID {request.Id} not found",
                        FinanceErrorCode.NotFound);
                }

                _logger.LogInformation("Retrieved GRN successfully: {GRNNumber}", grn.GRNNumber);

                return ResponseViewModel<GRNResponseDto>.Success(
                    grn,
                    "GRN retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching GRN with ID: {GRNId}", request.Id);
                throw;
            }
        }
    }
}
