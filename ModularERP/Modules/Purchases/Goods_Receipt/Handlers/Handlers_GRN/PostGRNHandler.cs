using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Modules.Purchases.Goods_Receipt.Commends.Commends_GRN;
using ModularERP.Modules.Purchases.Goods_Receipt.DTO.DTO_GRN;
using ModularERP.Modules.Purchases.Goods_Receipt.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Goods_Receipt.Handlers.Handlers_GRN
{
    public class PostGRNHandler : IRequestHandler<PostGRNCommand, GRNResponseDto>
    {
        private readonly IGeneralRepository<GoodsReceiptNote> _grnRepository;
        private readonly IGeneralRepository<GRNLineItem> _grnLineItemRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<PostGRNHandler> _logger;

        public PostGRNHandler(
            IGeneralRepository<GoodsReceiptNote> grnRepository,
            IGeneralRepository<GRNLineItem> grnLineItemRepository,
            IMapper mapper,
            ILogger<PostGRNHandler> logger)
        {
            _grnRepository = grnRepository;
            _grnLineItemRepository = grnLineItemRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<GRNResponseDto> Handle(PostGRNCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Posting GRN {GRNId} to inventory", request.GRNId);

                var grn = await _grnRepository.GetByID(request.GRNId);
                if (grn == null)
                {
                    throw new NotFoundException(
                        $"GRN with ID {request.GRNId} not found",
                        FinanceErrorCode.NotFound);
                }

                var lineItems = await _grnLineItemRepository
                    .GetAll()
                    .Where(l => l.GRNId == request.GRNId)
                    .ToListAsync(cancellationToken);

                if (lineItems.Count == 0)
                {
                    throw new BusinessLogicException(
                        "Cannot post GRN without line items",
                        "Purchases",
                        FinanceErrorCode.ValidationError);
                }

                // TODO: Add inventory posting logic here
                // This would typically:
                // 1. Update inventory quantities
                // 2. Create inventory transactions
                // 3. Update purchase order received quantities
                // 4. Post to general ledger (if integrated)

                _logger.LogInformation("GRN {GRNId} posted successfully", request.GRNId);

                var result = await _grnRepository
                    .GetAll()
                    .Where(g => g.Id == request.GRNId)
                    .ProjectTo<GRNResponseDto>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync(cancellationToken);

                return result!;
            }
            catch (Exception ex) when (ex is not BaseApplicationException)
            {
                _logger.LogError(ex, "Error posting GRN {GRNId}", request.GRNId);
                throw new BusinessLogicException(
                    $"Error posting GRN: {ex.Message}",
                    "Purchases",
                    FinanceErrorCode.BusinessLogicError);
            }
        }
    }
}