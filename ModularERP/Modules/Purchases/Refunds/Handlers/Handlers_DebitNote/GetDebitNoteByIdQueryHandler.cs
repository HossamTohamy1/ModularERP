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
    public class GetDebitNoteByIdQueryHandler : IRequestHandler<GetDebitNoteByIdQuery, ResponseViewModel<DebitNoteDetailsDto>>
    {
        private readonly IGeneralRepository<DebitNote> _debitNoteRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetDebitNoteByIdQueryHandler> _logger;

        public GetDebitNoteByIdQueryHandler(
            IGeneralRepository<DebitNote> debitNoteRepository,
            IMapper mapper,
            ILogger<GetDebitNoteByIdQueryHandler> logger)
        {
            _debitNoteRepository = debitNoteRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<DebitNoteDetailsDto>> Handle(
            GetDebitNoteByIdQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Fetching Debit Note with ID: {DebitNoteId}", request.Id);

                var debitNote = await _debitNoteRepository
                    .Get(x => x.Id == request.Id)
                    .ProjectTo<DebitNoteDetailsDto>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync(cancellationToken);

                if (debitNote == null)
                {
                    _logger.LogWarning("Debit Note not found with ID: {DebitNoteId}", request.Id);
                    throw new NotFoundException(
                        $"Debit Note with ID {request.Id} not found",
                        FinanceErrorCode.NotFound);
                }

                _logger.LogInformation("Successfully retrieved Debit Note: {DebitNoteNumber}",
                    debitNote.DebitNoteNumber);

                return ResponseViewModel<DebitNoteDetailsDto>.Success(
                    debitNote,
                    "Debit Note retrieved successfully");
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching Debit Note with ID: {DebitNoteId}", request.Id);
                throw new BusinessLogicException(
                    "Failed to retrieve Debit Note",
                    "Purchases.Refunds",
                    FinanceErrorCode.DatabaseError);
            }
        }
    }

}
