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
    public class GetDebitNotesBySupplierQueryHandler : IRequestHandler<GetDebitNotesBySupplierQuery, ResponseViewModel<List<SupplierDebitNoteDto>>>
    {
        private readonly IGeneralRepository<DebitNote> _debitNoteRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetDebitNotesBySupplierQueryHandler> _logger;

        public GetDebitNotesBySupplierQueryHandler(
            IGeneralRepository<DebitNote> debitNoteRepository,
            IMapper mapper,
            ILogger<GetDebitNotesBySupplierQueryHandler> logger)
        {
            _debitNoteRepository = debitNoteRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<List<SupplierDebitNoteDto>>> Handle(
            GetDebitNotesBySupplierQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Fetching Debit Notes for Supplier: {SupplierId}",
                    request.SupplierId);

                var query = _debitNoteRepository
                    .Get(x => x.SupplierId == request.SupplierId);

                // Apply date filters
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
                    .ProjectTo<SupplierDebitNoteDto>(_mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);

                _logger.LogInformation("Successfully retrieved {Count} Debit Notes for Supplier",
                    debitNotes.Count);

                return ResponseViewModel<List<SupplierDebitNoteDto>>.Success(
                    debitNotes,
                    $"Retrieved {debitNotes.Count} Debit Note(s) for supplier");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching Debit Notes for Supplier: {SupplierId}",
                    request.SupplierId);
                throw new BusinessLogicException(
                    "Failed to retrieve Debit Notes for supplier",
                    "Purchases.Refunds",
                    FinanceErrorCode.DatabaseError);
            }
        }
    }
}
