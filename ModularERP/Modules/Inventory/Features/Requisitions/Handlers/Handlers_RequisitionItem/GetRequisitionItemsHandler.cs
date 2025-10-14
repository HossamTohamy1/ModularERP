using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Requisitions.DTO.DTO_Requisition;
using ModularERP.Modules.Inventory.Features.Requisitions.Models;
using ModularERP.Modules.Inventory.Features.Requisitions.Qeuries.Qeuier_RequisitionItem;
using ModularERP.Shared.Interfaces;
using Serilog;
using ILogger = Serilog.ILogger;

namespace ModularERP.Modules.Inventory.Features.Requisitions.Handlers.Handlers_RequisitionItem
{
    public class GetRequisitionItemsHandler : IRequestHandler<GetRequisitionItemsQuery, ResponseViewModel<List<RequisitionItemDto>>>
    {
        private readonly IGeneralRepository<RequisitionItem> _repository;
        private readonly IGeneralRepository<Requisition> _requisitionRepository;
        private readonly ILogger _logger;
        public GetRequisitionItemsHandler(
            IGeneralRepository<RequisitionItem> repository,
            IGeneralRepository<Requisition> requisitionRepository)
        {
            _repository = repository;
            _requisitionRepository = requisitionRepository;
            _logger = Log.ForContext<GetRequisitionItemsHandler>();
        }

        public async Task<ResponseViewModel<List<RequisitionItemDto>>> Handle(GetRequisitionItemsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.Information("Retrieving items for requisition {RequisitionId}", request.RequisitionId);

                var requisitionExists = await _requisitionRepository.AnyAsync(
                    x => x.Id == request.RequisitionId,
                    cancellationToken
                );

                if (!requisitionExists)
                {
                    _logger.Warning("Requisition {RequisitionId} not found", request.RequisitionId);
                    throw new NotFoundException(
                        $"Requisition with ID {request.RequisitionId} not found",
                        FinanceErrorCode.NotFound
                    );
                }

                var items = await _repository.Get(x => x.RequisitionId == request.RequisitionId)
                    .Select(x => new RequisitionItemDto
                    {
                        Id = x.Id,
                        RequisitionId = x.RequisitionId,
                        ProductId = x.ProductId,
                        ProductName = x.Product.Name,
                        ProductSKU = x.Product.SKU,
                        UnitPrice = x.UnitPrice,
                        Quantity = x.Quantity,
                        //StockOnHand = x.StockOnHand,
                        //NewStockOnHand = x.NewStockOnHand,
                        LineTotal = x.LineTotal,
                        CreatedAt = x.CreatedAt
                    })
                    .ToListAsync(cancellationToken);

                _logger.Information("Retrieved {Count} items for requisition {RequisitionId}",
                    items.Count, request.RequisitionId);

                return ResponseViewModel<List<RequisitionItemDto>>.Success(
                    items,
                    "Requisition items retrieved successfully"
                );
            }
            catch (Exception ex) when (ex is not NotFoundException)
            {
                _logger.Error(ex, "Error retrieving items for requisition {RequisitionId}", request.RequisitionId);
                throw;
            }
        }
    }
}
