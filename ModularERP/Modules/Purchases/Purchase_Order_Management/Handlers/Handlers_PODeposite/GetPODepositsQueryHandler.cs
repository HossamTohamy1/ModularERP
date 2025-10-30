using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Purchase_Order_Management.DTO.DTO_PODeposite;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Models;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Qeuries.Qeuries_PODeposite;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Purchase_Order_Management.Handlers.Handlers_PODeposite
{
    public class GetPODepositsQueryHandler : IRequestHandler<GetPODepositsQuery, ResponseViewModel<List<PODepositResponseDto>>>
    {
        private readonly IGeneralRepository<PODeposit> _depositRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetPODepositsQueryHandler> _logger;

        public GetPODepositsQueryHandler(
            IGeneralRepository<PODeposit> depositRepository,
            IMapper mapper,
            ILogger<GetPODepositsQueryHandler> logger)
        {
            _depositRepository = depositRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<List<PODepositResponseDto>>> Handle(GetPODepositsQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Retrieving deposits for Purchase Order {PurchaseOrderId}", request.PurchaseOrderId);

            var deposits = await _depositRepository
                .Get(d => d.PurchaseOrderId == request.PurchaseOrderId)
                .OrderByDescending(d => d.CreatedAt)
                .ProjectTo<PODepositResponseDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            _logger.LogInformation("Retrieved {Count} deposits for Purchase Order {PurchaseOrderId}", deposits.Count, request.PurchaseOrderId);

            return ResponseViewModel<List<PODepositResponseDto>>.Success(deposits, "Deposits retrieved successfully");
        }
    }
}
