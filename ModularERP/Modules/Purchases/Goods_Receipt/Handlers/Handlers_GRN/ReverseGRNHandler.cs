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
    public class ReverseGRNHandler : IRequestHandler<ReverseGRNCommand, GRNResponseDto>
    {
        private readonly IGeneralRepository<GoodsReceiptNote> _grnRepository;
        private readonly IGeneralRepository<GRNLineItem> _grnLineItemRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<ReverseGRNHandler> _logger;

        public ReverseGRNHandler(
            IGeneralRepository<GoodsReceiptNote> grnRepository,
            IGeneralRepository<GRNLineItem> grnLineItemRepository,
            IMapper mapper,
            ILogger<ReverseGRNHandler> logger)
        {
            _grnRepository = grnRepository;
            _grnLineItemRepository = grnLineItemRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<GRNResponseDto> Handle(ReverseGRNCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Reversing GRN {GRNId}", request.GRNId);

                var grn = await _grnRepository.GetByIDWithTracking(request.GRNId);
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
                        "Cannot reverse GRN without line items",
                        "Purchases",
                        FinanceErrorCode.ValidationError);
                }

                // TODO: Add inventory reversal logic here
                // This would typically:
                // 1. Decrease inventory quantities
                // 2. Create negative inventory transactions
                // 3. Update purchase order received quantities (decrease)
                // 4. Reverse general ledger entries (if integrated)

                grn.Notes = string.IsNullOrEmpty(grn.Notes)
                    ? $"Reversed: {request.ReversalReason}"
                    : $"{grn.Notes}\n\nReversed: {request.ReversalReason}";

                grn.UpdatedById = request.UserId;
                grn.UpdatedAt = DateTime.UtcNow;

                await _grnRepository.SaveChanges();

                _logger.LogInformation("GRN {GRNId} reversed successfully", request.GRNId);

                var result = await _grnRepository
                    .GetAll()
                    .Where(g => g.Id == request.GRNId)
                    .ProjectTo<GRNResponseDto>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync(cancellationToken);

                return result!;
            }
            catch (Exception ex) when (ex is not BaseApplicationException)
            {
                _logger.LogError(ex, "Error reversing GRN {GRNId}", request.GRNId);
                throw new BusinessLogicException(
                    $"Error reversing GRN: {ex.Message}",
                    "Purchases",
                    FinanceErrorCode.BusinessLogicError);
            }
        }
    }
}