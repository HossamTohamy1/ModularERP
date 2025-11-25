using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Refunds.DTO.DTO_DebitNote;
using ModularERP.Modules.Purchases.Refunds.Models;
using ModularERP.Modules.Purchases.Refunds.Qeuries.Queries_DebitNote;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Refunds.Handlers.Handlers_DebitNote
{
    public class GetAllDebitNotesQueryHandler : IRequestHandler<GetAllDebitNotesQuery, ResponseViewModel<List<DebitNoteListDto>>>
    {
        private readonly IGeneralRepository<DebitNote> _debitNoteRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetAllDebitNotesQueryHandler> _logger;

        public GetAllDebitNotesQueryHandler(
            IGeneralRepository<DebitNote> debitNoteRepository,
            IMapper mapper,
            ILogger<GetAllDebitNotesQueryHandler> logger)
        {
            _debitNoteRepository = debitNoteRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<List<DebitNoteListDto>>> Handle(
            GetAllDebitNotesQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Fetching Debit Notes - Page: {PageNumber}, Size: {PageSize}",
                    request.PageNumber, request.PageSize);

                var query = _debitNoteRepository.GetAll();

                // Apply filters
                if (request.SupplierId.HasValue)
                {
                    query = query.Where(x => x.SupplierId == request.SupplierId.Value);
                }

                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    var searchLower = request.SearchTerm.ToLower();
                    query = query.Where(x =>
                        x.DebitNoteNumber.ToLower().Contains(searchLower) ||
                        x.Notes != null && x.Notes.ToLower().Contains(searchLower));
                }

                if (request.FromDate.HasValue)
                {
                    query = query.Where(x => x.NoteDate >= request.FromDate.Value);
                }

                if (request.ToDate.HasValue)
                {
                    query = query.Where(x => x.NoteDate <= request.ToDate.Value);
                }

                // Order by date descending
                query = query.OrderByDescending(x => x.NoteDate);

                // Apply pagination and projection
                var debitNotes = await query
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ProjectTo<DebitNoteListDto>(_mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);

                _logger.LogInformation("Successfully retrieved {Count} Debit Notes", debitNotes.Count);

                return ResponseViewModel<List<DebitNoteListDto>>.Success(
                    debitNotes,
                    $"Retrieved {debitNotes.Count} Debit Note(s)");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching Debit Notes");
                throw new BusinessLogicException(
                    "Failed to retrieve Debit Notes",
                    "Purchases.Refunds",
                    FinanceErrorCode.DatabaseError);
            }
        }
    }
}