using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Payment.DTO.DTO_PaymentTerm;
using ModularERP.Modules.Purchases.Payment.Models;
using ModularERP.Modules.Purchases.Payment.Qeuries.Queries_PaymentTerm;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Payment.Handlers.Handlers_PaymentTerm
{
    public class GetAllPaymentTermsHandler : IRequestHandler<GetAllPaymentTermsQuery, ResponseViewModel<List<PaymentTermResponseDto>>>
    {
        private readonly IGeneralRepository<PaymentTerm> _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetAllPaymentTermsHandler> _logger;

        public GetAllPaymentTermsHandler(
            IGeneralRepository<PaymentTerm> repository,
            IMapper mapper,
            ILogger<GetAllPaymentTermsHandler> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<List<PaymentTermResponseDto>>> Handle(
            GetAllPaymentTermsQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Fetching all Payment Terms");

                // Build query with projection (NO INCLUDE)
                var query = _repository.GetAll();

                // Filter by IsActive if provided
                if (request.IsActive.HasValue)
                {
                    query = query.Where(x => x.IsActive == request.IsActive.Value);
                }

                // Use AutoMapper projection instead of Include
                var paymentTerms = await query
                    .OrderBy(x => x.Days)
                    .ProjectTo<PaymentTermResponseDto>(_mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);

                _logger.LogInformation("Retrieved {Count} Payment Terms", paymentTerms.Count);

                return ResponseViewModel<List<PaymentTermResponseDto>>.Success(
                    paymentTerms,
                    "Payment Terms retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching Payment Terms");
                throw new BusinessLogicException(
                    "An error occurred while fetching Payment Terms",
                    ex,
                    "Purchases");
            }
        }
    }
}