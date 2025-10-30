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
    public class GetPOLineItemByIdHandler : IRequestHandler<GetPOLineItemByIdQuery, ResponseViewModel<POLineItemResponseDto>>
    {
        private readonly IGeneralRepository<POLineItem> _lineItemRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetPOLineItemByIdHandler> _logger;

        public GetPOLineItemByIdHandler(
            IGeneralRepository<POLineItem> lineItemRepository,
            IMapper mapper,
            ILogger<GetPOLineItemByIdHandler> logger)
        {
            _lineItemRepository = lineItemRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<POLineItemResponseDto>> Handle(
            GetPOLineItemByIdQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Retrieving line item {LineItemId} for Purchase Order {PurchaseOrderId}",
                    request.LineItemId, request.PurchaseOrderId);

                var lineItem = await _lineItemRepository
                    .Get(x => x.Id == request.LineItemId && x.PurchaseOrderId == request.PurchaseOrderId)
                    .ProjectTo<POLineItemResponseDto>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync(cancellationToken);

                if (lineItem == null)
                {
                    throw new NotFoundException(
                        $"Line item with ID {request.LineItemId} not found for Purchase Order {request.PurchaseOrderId}",
                        FinanceErrorCode.NotFound);
                }

                return ResponseViewModel<POLineItemResponseDto>.Success(
                    lineItem,
                    "Line item retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving line item {LineItemId}", request.LineItemId);
                throw;
            }
        }
    }
}
