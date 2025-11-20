using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Refunds.DTO.DTO_RefundInvoce;
using ModularERP.Modules.Purchases.Refunds.Models;
using ModularERP.Modules.Purchases.Refunds.Queries.Queries_RefundItem;  
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Refunds.Handlers.Handlers_RefundItem
{
    public class GetRefundDebitNoteHandler : IRequestHandler<GetRefundDebitNoteQuery, ResponseViewModel<DebitNoteDto>>
    {
        private readonly IGeneralRepository<DebitNote> _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetRefundDebitNoteHandler> _logger;

        public GetRefundDebitNoteHandler(
            IGeneralRepository<DebitNote> repository,
            IMapper mapper,
            ILogger<GetRefundDebitNoteHandler> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<DebitNoteDto>> Handle(GetRefundDebitNoteQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Getting Debit Note for Refund: {RefundId}", request.RefundId);

                var debitNote = await _repository
                    .Get(dn => dn.RefundId == request.RefundId)
                    .ProjectTo<DebitNoteDto>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync(cancellationToken);

                if (debitNote == null)
                {
                    throw new NotFoundException(
                        "Debit Note not found for this refund. The refund may not have been posted yet.",
                        FinanceErrorCode.NotFound);
                }

                _logger.LogInformation(
                    "Debit Note retrieved successfully. DN: {DebitNoteNumber}",
                    debitNote.DebitNoteNumber);

                return ResponseViewModel<DebitNoteDto>.Success(
                    debitNote,
                    "Debit Note retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Debit Note for Refund: {RefundId}", request.RefundId);
                throw;
            }
        }
    }
}
