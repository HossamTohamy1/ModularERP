using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Modules.Purchases.Goods_Receipt.DTO.DTO_GRN;
using ModularERP.Modules.Purchases.Goods_Receipt.Models;
using ModularERP.Modules.Purchases.Goods_Receipt.Qeuries.Qeuires_GRNPO;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Goods_Receipt.Handlers.Handlers_GRNPO
{
    public class GetGRNsByPOHandler : IRequestHandler<GetGRNsByPOQuery, List<GRNListItemDto>>
    {
        private readonly IGeneralRepository<GoodsReceiptNote> _grnRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetGRNsByPOHandler> _logger;

        public GetGRNsByPOHandler(
            IGeneralRepository<GoodsReceiptNote> grnRepository,
            IMapper mapper,
            ILogger<GetGRNsByPOHandler> logger)
        {
            _grnRepository = grnRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<List<GRNListItemDto>> Handle(GetGRNsByPOQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Getting GRNs for PO {PurchaseOrderId}", request.PurchaseOrderId);

                var grns = await _grnRepository
                    .GetByCompanyId(request.CompanyId)
                    .Where(g => g.PurchaseOrderId == request.PurchaseOrderId)
                    .OrderByDescending(g => g.CreatedAt)
                    .ProjectTo<GRNListItemDto>(_mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);

                _logger.LogInformation("Found {Count} GRNs for PO {PurchaseOrderId}", grns.Count, request.PurchaseOrderId);

                return grns;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting GRNs for PO {PurchaseOrderId}", request.PurchaseOrderId);
                throw new BusinessLogicException(
                    $"Error retrieving GRNs for Purchase Order: {ex.Message}",
                    "Purchases",
                    FinanceErrorCode.BusinessLogicError);
            }
        }
    }
}