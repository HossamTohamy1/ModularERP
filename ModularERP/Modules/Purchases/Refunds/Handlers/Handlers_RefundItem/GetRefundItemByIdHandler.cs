using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Refunds.DTO.DTO_RefundItem;
using ModularERP.Modules.Purchases.Refunds.Models;
using ModularERP.Modules.Purchases.Refunds.Qeuries.Qeuries_RefundItem;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Refunds.Handlers.Handlers_RefundItem
{
    // Get Refund Item By ID Handler
    public class GetRefundItemByIdHandler : IRequestHandler<GetRefundItemByIdQuery, ResponseViewModel<RefundItemDto>>
    {
        private readonly IGeneralRepository<RefundLineItem> _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetRefundItemByIdHandler> _logger;

        public GetRefundItemByIdHandler(
            IGeneralRepository<RefundLineItem> repository,
            IMapper mapper,
            ILogger<GetRefundItemByIdHandler> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<RefundItemDto>> Handle(GetRefundItemByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Getting refund item: {ItemId} for Refund: {RefundId}", request.ItemId, request.RefundId);

                var item = await _repository.GetAll()
                    .Where(r => r.Id == request.ItemId && r.RefundId == request.RefundId)
                    .Include(r => r.GRNLineItem)
                        .ThenInclude(g => g.POLineItem)
                        .ThenInclude(pol => pol.Product)
                    .Select(rl => new RefundItemDto
                    {
                        Id = rl.Id,
                        RefundId = rl.RefundId,
                        GRNLineItemId = rl.GRNLineItemId,
                        ProductId = rl.GRNLineItem.POLineItem.ProductId ?? Guid.Empty,
                        ProductName = rl.GRNLineItem.POLineItem.Product != null ? rl.GRNLineItem.POLineItem.Product.Name : "",
                        ProductSKU = rl.GRNLineItem.POLineItem.Product != null ? rl.GRNLineItem.POLineItem.Product.SKU : "",
                        ReturnQuantity = rl.ReturnQuantity,
                        UnitPrice = rl.UnitPrice,
                        LineTotal = rl.LineTotal
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                if (item == null)
                {
                    throw new NotFoundException("Refund item not found", FinanceErrorCode.NotFound);
                }

                _logger.LogInformation("Refund item retrieved successfully. Item ID: {ItemId}", request.ItemId);

                return ResponseViewModel<RefundItemDto>.Success(item, "Refund item retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting refund item: {ItemId}", request.ItemId);
                throw;
            }
        }
    }
}