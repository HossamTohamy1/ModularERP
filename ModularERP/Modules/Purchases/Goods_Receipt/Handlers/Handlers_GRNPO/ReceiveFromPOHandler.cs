using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Modules.Purchases.Goods_Receipt.Commends.Commends_GRNPO;
using ModularERP.Modules.Purchases.Goods_Receipt.DTO.DTO_GRN;
using ModularERP.Modules.Purchases.Goods_Receipt.Models;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Goods_Receipt.Handlers.Handlers_GRNPO
{
    public class ReceiveFromPOHandler : IRequestHandler<ReceiveFromPOCommand, GRNResponseDto>
    {
        private readonly IGeneralRepository<GoodsReceiptNote> _grnRepository;
        private readonly IGeneralRepository<PurchaseOrder> _poRepository;
        private readonly IGeneralRepository<POLineItem> _poLineItemRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<ReceiveFromPOHandler> _logger;

        public ReceiveFromPOHandler(
            IGeneralRepository<GoodsReceiptNote> grnRepository,
            IGeneralRepository<PurchaseOrder> poRepository,
            IGeneralRepository<POLineItem> poLineItemRepository,
            IMapper mapper,
            ILogger<ReceiveFromPOHandler> logger)
        {
            _grnRepository = grnRepository;
            _poRepository = poRepository;
            _poLineItemRepository = poLineItemRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<GRNResponseDto> Handle(ReceiveFromPOCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Creating GRN for PO {PurchaseOrderId}", request.PurchaseOrderId);

                var purchaseOrder = await _poRepository.GetByID(request.PurchaseOrderId);
                if (purchaseOrder == null)
                {
                    throw new NotFoundException(
                        $"Purchase Order with ID {request.PurchaseOrderId} not found",
                        FinanceErrorCode.NotFound);
                }

                var poLineItemIds = request.LineItems.Select(l => l.POLineItemId).ToList();
                var poLineItems = await _poLineItemRepository
                    .GetAll()
                    .Where(p => poLineItemIds.Contains(p.Id) && p.PurchaseOrderId == request.PurchaseOrderId)
                    .ToListAsync(cancellationToken);

                if (poLineItems.Count != request.LineItems.Count)
                {
                    throw new BusinessLogicException(
                        "Some line items do not belong to this Purchase Order",
                        "Purchases",
                        FinanceErrorCode.ValidationError);
                }

                var grnNumber = await GenerateGRNNumber(request.CompanyId, cancellationToken);

                var grn = new GoodsReceiptNote
                {
                    Id = Guid.NewGuid(),
                    GRNNumber = grnNumber,
                    PurchaseOrderId = request.PurchaseOrderId,
                    WarehouseId = request.WarehouseId,
                    CompanyId = request.CompanyId,
                    ReceiptDate = request.ReceiptDate,
                    ReceivedBy = request.ReceivedBy,
                    Notes = request.Notes,
                    CreatedById = request.UserId,
                    CreatedAt = DateTime.UtcNow
                };

                grn.LineItems = request.LineItems.Select(item => new GRNLineItem
                {
                    Id = Guid.NewGuid(),
                    GRNId = grn.Id,
                    POLineItemId = item.POLineItemId,
                    ReceivedQuantity = item.ReceivedQuantity,
                    Notes = item.Notes,
                    CreatedById = request.UserId,
                    CreatedAt = DateTime.UtcNow
                }).ToList();

                await _grnRepository.AddAsync(grn);
                await _grnRepository.SaveChanges();

                _logger.LogInformation("GRN {GRNNumber} created successfully for PO {PurchaseOrderId}",
                    grnNumber, request.PurchaseOrderId);

                var result = await _grnRepository
                    .GetAll()
                    .Where(g => g.Id == grn.Id)
                    .ProjectTo<GRNResponseDto>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync(cancellationToken);

                return result!;
            }
            catch (Exception ex) when (ex is not BaseApplicationException)
            {
                _logger.LogError(ex, "Error creating GRN for PO {PurchaseOrderId}", request.PurchaseOrderId);
                throw new BusinessLogicException(
                    $"Error creating GRN: {ex.Message}",
                    "Purchases",
                    FinanceErrorCode.BusinessLogicError);
            }
        }

        private async Task<string> GenerateGRNNumber(Guid companyId, CancellationToken cancellationToken)
        {
            var lastGRN = await _grnRepository
                .GetByCompanyId(companyId)
                .OrderByDescending(g => g.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (lastGRN == null)
            {
                return $"GRN-{DateTime.UtcNow:yyyyMMdd}-0001";
            }

            var lastNumber = lastGRN.GRNNumber.Split('-').Last();
            if (int.TryParse(lastNumber, out var number))
            {
                return $"GRN-{DateTime.UtcNow:yyyyMMdd}-{(number + 1):D4}";
            }

            return $"GRN-{DateTime.UtcNow:yyyyMMdd}-0001";
        }
    }
}