using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Refunds.DTO.DTO_DebitNote;
using ModularERP.Modules.Purchases.Refunds.Models;
using ModularERP.Modules.Purchases.Refunds.Qeuries.Queries_DebitNote;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Refunds.Handlers.Handlers_DebitNote
{
    public class GetDebitNoteRefundQueryHandler : IRequestHandler<GetDebitNoteRefundQuery, ResponseViewModel<RefundSummaryDto>>
    {
        private readonly IGeneralRepository<DebitNote> _debitNoteRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetDebitNoteRefundQueryHandler> _logger;

        public GetDebitNoteRefundQueryHandler(
            IGeneralRepository<DebitNote> debitNoteRepository,
            IMapper mapper,
            ILogger<GetDebitNoteRefundQueryHandler> logger)
        {
            _debitNoteRepository = debitNoteRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<RefundSummaryDto>> Handle(
            GetDebitNoteRefundQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Fetching Refund for Debit Note: {DebitNoteId}",
                    request.DebitNoteId);

                var refund = await _debitNoteRepository
                    .Get(x => x.Id == request.DebitNoteId)
                    .Select(x => x.Refund)
                    .ProjectTo<RefundSummaryDto>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync(cancellationToken);

                if (refund == null)
                {
                    _logger.LogWarning("Refund not found for Debit Note: {DebitNoteId}",
                        request.DebitNoteId);
                    throw new NotFoundException(
                        $"Refund not found for Debit Note {request.DebitNoteId}",
                        FinanceErrorCode.NotFound);
                }

                _logger.LogInformation("Successfully retrieved Refund: {RefundNumber}",
                    refund.RefundNumber);

                return ResponseViewModel<RefundSummaryDto>.Success(
                    refund,
                    "Refund retrieved successfully");
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching Refund for Debit Note: {DebitNoteId}",
                    request.DebitNoteId);
                throw new BusinessLogicException(
                    "Failed to retrieve Refund",
                    "Purchases.Refunds",
                    FinanceErrorCode.DatabaseError);
            }
        }
    }
}