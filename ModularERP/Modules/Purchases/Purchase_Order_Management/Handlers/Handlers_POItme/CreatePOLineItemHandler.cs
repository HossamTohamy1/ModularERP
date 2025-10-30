using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Commends.Commends_POItme;
using ModularERP.Modules.Purchases.Purchase_Order_Management.DTO.DTO_POItme;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Models;
using ModularERP.Shared.Interfaces;
using Serilog;

namespace ModularERP.Modules.Purchases.Purchase_Order_Management.Handlers.Handlers_POItme
{
    public class CreatePOLineItemHandler : IRequestHandler<CreatePOLineItemCommand, ResponseViewModel<POLineItemResponseDto>>
    {
        private readonly IGeneralRepository<POLineItem> _lineItemRepository;
        private readonly IGeneralRepository<PurchaseOrder> _poRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<CreatePOLineItemHandler> _logger;

        public CreatePOLineItemHandler(
            IGeneralRepository<POLineItem> lineItemRepository,
            IGeneralRepository<PurchaseOrder> poRepository,
            IMapper mapper,
            ILogger<CreatePOLineItemHandler> logger)
        {
            _lineItemRepository = lineItemRepository;
            _poRepository = poRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<POLineItemResponseDto>> Handle(
            CreatePOLineItemCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Creating line item for Purchase Order {PurchaseOrderId}", request.PurchaseOrderId);

                // Verify PO exists and is in valid state
                var po = await _poRepository.GetByID(request.PurchaseOrderId);
                if (po == null)
                {
                    throw new NotFoundException(
                        $"Purchase Order with ID {request.PurchaseOrderId} not found",
                        FinanceErrorCode.NotFound);
                }

                // Check if PO is in Draft or Submitted status (can add items)
                if (po.DocumentStatus != "Draft" && po.DocumentStatus != "Submitted")
                {
                    throw new BusinessLogicException(
                        $"Cannot add items to Purchase Order in {po.DocumentStatus} status",
                        "Purchases",
                        FinanceErrorCode.BusinessLogicError);
                }

                // Map DTO to entity
                var lineItem = _mapper.Map<POLineItem>(request.LineItem);
                lineItem.PurchaseOrderId = request.PurchaseOrderId;
                lineItem.CreatedAt = DateTime.UtcNow;
                lineItem.IsActive = true;
                lineItem.IsDeleted = false;

                // Calculate remaining quantity
                lineItem.RemainingQuantity = lineItem.Quantity;

                // Add to repository
                await _lineItemRepository.AddAsync(lineItem);
                await _lineItemRepository.SaveChanges();

                _logger.LogInformation("Line item {LineItemId} created successfully for PO {PurchaseOrderId}",
                    lineItem.Id, request.PurchaseOrderId);

                // Retrieve with projections
                var response = await _lineItemRepository
                    .Get(x => x.Id == lineItem.Id)
                    .ProjectTo<POLineItemResponseDto>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync(cancellationToken);

                return ResponseViewModel<POLineItemResponseDto>.Success(
                    response!,
                    "Line item created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating line item for Purchase Order {PurchaseOrderId}",
                    request.PurchaseOrderId);
                throw;
            }
        }
    }
}
