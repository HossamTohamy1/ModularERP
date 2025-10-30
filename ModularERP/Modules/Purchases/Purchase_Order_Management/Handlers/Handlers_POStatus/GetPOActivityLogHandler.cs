using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Purchase_Order_Management.DTO.DTO_POStatus;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Qeuries.Qeuries_POStauts;
using ModularERP.Modules.Purchases.WorkFlow.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Purchase_Order_Management.Handlers.Handlers_POStatus
{
    public class GetPOActivityLogHandler : IRequestHandler<GetPOActivityLogQuery, ResponseViewModel<List<POActivityLogDto>>>
    {
        private readonly IGeneralRepository<POAuditLog> _repo;
        private readonly IMapper _mapper;
        private readonly ILogger<GetPOActivityLogHandler> _logger;

        public GetPOActivityLogHandler(
            IGeneralRepository<POAuditLog> repo,
            IMapper mapper,
            ILogger<GetPOActivityLogHandler> logger)
        {
            _repo = repo;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<List<POActivityLogDto>>> Handle(
            GetPOActivityLogQuery request,
            CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Fetching activity log for PO ID: {POId}", request.PurchaseOrderId);

                var logs = await _repo.GetAll()
                    .Where(x => x.PurchaseOrderId == request.PurchaseOrderId)
                    .OrderByDescending(x => x.CreatedAt)
                    .ProjectTo<POActivityLogDto>(_mapper.ConfigurationProvider)
                    .ToListAsync(ct);

                _logger.LogInformation("Retrieved {Count} activity log entries", logs.Count);
                return ResponseViewModel<List<POActivityLogDto>>.Success(
                    logs,
                    "Activity log retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving activity log for PO ID: {POId}", request.PurchaseOrderId);
                throw new BusinessLogicException(
                    "Error retrieving purchase order activity log",
                    ex,
                    "PurchaseOrders",
                    FinanceErrorCode.BusinessLogicError);
            }
        }
    }

}
