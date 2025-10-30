using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Purchase_Order_Management.DTO.DTO_POStatus;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Models;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Qeuries.Qeuries_POStauts;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Purchase_Order_Management.Handlers.Handlers_POStatus
{
    public class GetPOStatusHandler : IRequestHandler<GetPOStatusQuery, ResponseViewModel<POStatusDto>>
    {
        private readonly IGeneralRepository<PurchaseOrder> _repo;
        private readonly IMapper _mapper;
        private readonly ILogger<GetPOStatusHandler> _logger;

        public GetPOStatusHandler(
            IGeneralRepository<PurchaseOrder> repo,
            IMapper mapper,
            ILogger<GetPOStatusHandler> logger)
        {
            _repo = repo;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<POStatusDto>> Handle(
            GetPOStatusQuery request,
            CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Fetching PO status for ID: {POId}", request.PurchaseOrderId);

                var status = await _repo.GetAll()
                    .Where(x => x.Id == request.PurchaseOrderId)
                    .ProjectTo<POStatusDto>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync(ct);

                if (status == null)
                {
                    throw new NotFoundException(
                        $"Purchase Order with ID {request.PurchaseOrderId} not found",
                        FinanceErrorCode.NotFound);
                }

                _logger.LogInformation("Successfully retrieved PO status for {PONumber}", status.PONumber);
                return ResponseViewModel<POStatusDto>.Success(status, "PO status retrieved successfully");
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving PO status for ID: {POId}", request.PurchaseOrderId);
                throw new BusinessLogicException(
                    "Error retrieving purchase order status",
                    ex,
                    "PurchaseOrders",
                    FinanceErrorCode.BusinessLogicError);
            }
        }
    }
}
