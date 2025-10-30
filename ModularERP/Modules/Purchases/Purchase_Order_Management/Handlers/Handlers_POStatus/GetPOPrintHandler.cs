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
    public class GetPOPrintHandler : IRequestHandler<GetPOPrintQuery, ResponseViewModel<POPrintDto>>
    {
        private readonly IGeneralRepository<PurchaseOrder> _repo;
        private readonly IMapper _mapper;
        private readonly ILogger<GetPOPrintHandler> _logger;

        public GetPOPrintHandler(
            IGeneralRepository<PurchaseOrder> repo,
            IMapper mapper,
            ILogger<GetPOPrintHandler> logger)
        {
            _repo = repo;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<POPrintDto>> Handle(
            GetPOPrintQuery request,
            CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Fetching print data for PO ID: {POId}", request.PurchaseOrderId);

                var printData = await _repo.GetAll()
                    .Where(x => x.Id == request.PurchaseOrderId)
                    .ProjectTo<POPrintDto>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync(ct);

                if (printData == null)
                {
                    throw new NotFoundException(
                        $"Purchase Order with ID {request.PurchaseOrderId} not found",
                        FinanceErrorCode.NotFound);
                }

                // Add line numbers
                for (int i = 0; i < printData.LineItems.Count; i++)
                {
                    printData.LineItems[i].LineNumber = i + 1;
                }

                _logger.LogInformation("Successfully retrieved print data for {PONumber}", printData.PONumber);
                return ResponseViewModel<POPrintDto>.Success(printData, "Print data retrieved successfully");
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving print data for PO ID: {POId}", request.PurchaseOrderId);
                throw new BusinessLogicException(
                    "Error retrieving purchase order print data",
                    ex,
                    "PurchaseOrders",
                    FinanceErrorCode.BusinessLogicError);
            }
        }
    }
}

