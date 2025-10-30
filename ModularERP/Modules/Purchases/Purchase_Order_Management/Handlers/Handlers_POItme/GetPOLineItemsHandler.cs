using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Purchase_Order_Management.DTO.DTO_POItme;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Models;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Qeuries.Qeuries_POItme;
using ModularERP.Shared.Interfaces;
using Serilog;

namespace ModularERP.Modules.Purchases.Purchase_Order_Management.Handlers.Handlers_POItme
{
    public class GetPOLineItemsHandler : IRequestHandler<GetPOLineItemsQuery, ResponseViewModel<List<POLineItemListDto>>>
    {
        private readonly IGeneralRepository<POLineItem> _lineItemRepository;
        private readonly IGeneralRepository<PurchaseOrder> _poRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetPOLineItemsHandler> _logger;

        public GetPOLineItemsHandler(
            IGeneralRepository<POLineItem> lineItemRepository,
            IGeneralRepository<PurchaseOrder> poRepository,
            IMapper mapper,
            ILogger<GetPOLineItemsHandler> logger)
        {
            _lineItemRepository = lineItemRepository;
            _poRepository = poRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<List<POLineItemListDto>>> Handle(
            GetPOLineItemsQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Retrieving all line items for Purchase Order {PurchaseOrderId}",
                    request.PurchaseOrderId);

                // Verify PO exists
                var poExists = await _poRepository.AnyAsync(
                    po => po.Id == request.PurchaseOrderId,
                    cancellationToken);

                if (!poExists)
                {
                    throw new NotFoundException(
                        $"Purchase Order with ID {request.PurchaseOrderId} not found",
                        FinanceErrorCode.NotFound);
                }

                var lineItems = await _lineItemRepository
                    .Get(x => x.PurchaseOrderId == request.PurchaseOrderId)
                    .OrderBy(x => x.CreatedAt)
                    .ProjectTo<POLineItemListDto>(_mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);

                return ResponseViewModel<List<POLineItemListDto>>.Success(
                    lineItems,
                    $"Retrieved {lineItems.Count} line items successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving line items for Purchase Order {PurchaseOrderId}",
                    request.PurchaseOrderId);
                throw;
            }
        }
    }

}
