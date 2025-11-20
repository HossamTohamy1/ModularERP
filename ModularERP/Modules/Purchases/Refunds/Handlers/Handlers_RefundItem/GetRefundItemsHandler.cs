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
    public class GetRefundItemsHandler : IRequestHandler<GetRefundItemsQuery, ResponseViewModel<List<RefundItemListDto>>>
    {
        private readonly IGeneralRepository<RefundLineItem> _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetRefundItemsHandler> _logger;

        public GetRefundItemsHandler(
            IGeneralRepository<RefundLineItem> repository,
            IMapper mapper,
            ILogger<GetRefundItemsHandler> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<List<RefundItemListDto>>> Handle(GetRefundItemsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Getting refund items for Refund: {RefundId}", request.RefundId);

                var items = await _repository
                    .Get(r => r.RefundId == request.RefundId)
                    .ProjectTo<RefundItemListDto>(_mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);

                _logger.LogInformation("Retrieved {Count} refund items for Refund: {RefundId}", items.Count, request.RefundId);

                return ResponseViewModel<List<RefundItemListDto>>.Success(items, "Refund items retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting refund items for Refund: {RefundId}", request.RefundId);
                throw;
            }
        }
    }

   
}