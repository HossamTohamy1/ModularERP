using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Purchase_Order_Management.DTO.DTO_PurchaseOrder;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Models;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Qeuries.Qeuries_PurchaseOrder;
using ModularERP.Shared.Interfaces;
using Serilog;

namespace ModularERP.Modules.Purchases.Purchase_Order_Management.Handlers.Handlers_PurchaseOrder
{
    public class GetPurchaseOrderHandler : IRequestHandler<GetPurchaseOrderQuery, ResponseViewModel<PurchaseOrderDetailDto>>
    {
        private readonly IGeneralRepository<PurchaseOrder> _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetPurchaseOrderHandler> _logger;
        public GetPurchaseOrderHandler(
        IGeneralRepository<PurchaseOrder> repository,
        ILogger<GetPurchaseOrderHandler> logger,
        IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }
        public async Task<ResponseViewModel<PurchaseOrderDetailDto>> Handle(
        GetPurchaseOrderQuery request,
        CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Retrieving purchase order with ID: {POId}", request.Id);
                var purchaseOrder = await _repository
                .GetAll()
                .Where(x => x.Id == request.Id)
                .ProjectTo<PurchaseOrderDetailDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);

                if (purchaseOrder == null)
                {
                    _logger.LogWarning("Purchase order not found with ID: {POId}", request.Id);
                    throw new NotFoundException(
                        $"Purchase order with ID {request.Id} not found",
                        FinanceErrorCode.NotFound);
                }

                _logger.LogInformation("Purchase order retrieved successfully: {PONumber}", purchaseOrder.PONumber);

                return ResponseViewModel<PurchaseOrderDetailDto>.Success(
                    purchaseOrder,
                    "Purchase order retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving purchase order with ID: {POId}", request.Id);
                throw;
            }
        }
    }
}