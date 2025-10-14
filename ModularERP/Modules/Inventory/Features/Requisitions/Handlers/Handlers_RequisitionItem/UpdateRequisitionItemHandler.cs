using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Enum.Inventory_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Finance.Infrastructure.Data;
using ModularERP.Modules.Inventory.Features.Requisitions.Commends.Commends_RequisitionItem;
using ModularERP.Modules.Inventory.Features.Requisitions.DTO.DTO_Requisition;
using ModularERP.Modules.Inventory.Features.Requisitions.Models;
using ModularERP.Shared.Interfaces;
using Serilog;
using ILogger = Serilog.ILogger;

namespace ModularERP.Modules.Inventory.Features.Requisitions.Handlers.Handlers_RequisitionItem
{
    public class UpdateRequisitionItemHandler : IRequestHandler<UpdateRequisitionItemCommand, ResponseViewModel<RequisitionItemDto>>
    {
        private readonly IGeneralRepository<RequisitionItem> _repository;
        private readonly IGeneralRepository<Requisition> _requisitionRepository;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;
        private readonly FinanceDbContext _dbContext;

        public UpdateRequisitionItemHandler(
            IGeneralRepository<RequisitionItem> repository,
            IGeneralRepository<Requisition> requisitionRepository,
            IMapper mapper,
            FinanceDbContext dbContext)
        {
            _repository = repository;
            _requisitionRepository = requisitionRepository;
            _mapper = mapper;
            _dbContext = dbContext;
            _logger = Log.ForContext<UpdateRequisitionItemHandler>();
        }

        public async Task<ResponseViewModel<RequisitionItemDto>> Handle(UpdateRequisitionItemCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.Information("Updating item {ItemId} for requisition {RequisitionId}",
                    request.ItemId, request.RequisitionId);

                var existingItem = await _repository.GetByIDWithTracking(request.ItemId);
                if (existingItem == null || existingItem.RequisitionId != request.RequisitionId)
                {
                    _logger.Warning("Item {ItemId} not found in requisition {RequisitionId}",
                        request.ItemId, request.RequisitionId);
                    throw new NotFoundException(
                        $"Item with ID {request.ItemId} not found in requisition {request.RequisitionId}",
                        FinanceErrorCode.NotFound
                    );
                }

                var requisition = await _requisitionRepository.GetByID(request.RequisitionId);
                if (requisition == null)
                {
                    _logger.Warning("Requisition {RequisitionId} not found", request.RequisitionId);
                    throw new NotFoundException(
                        $"Requisition with ID {request.RequisitionId} not found",
                        FinanceErrorCode.NotFound
                    );
                }

                if (requisition.Status != RequisitionStatus.Draft)
                {
                    _logger.Warning("Cannot update items in requisition {RequisitionId} with status {Status}",
                        request.RequisitionId, requisition.Status);
                    throw new BusinessLogicException(
                        "Cannot update items in a requisition that is not in Draft status",
                        "Inventory",
                        FinanceErrorCode.BusinessLogicError
                    );
                }


                var warehouseStock = await _dbContext.WarehouseStocks
                    .Where(ws => ws.WarehouseId == requisition.WarehouseId && ws.ProductId == request.Item.ProductId)
                    .FirstOrDefaultAsync(cancellationToken);

                if (warehouseStock == null)
                {
                    _logger.Warning("Product {ProductId} not found in warehouse {WarehouseId}",
                        request.Item.ProductId, requisition.WarehouseId);
                    throw new BusinessLogicException(
                        $"Product with ID {request.Item.ProductId} is not available in warehouse {requisition.WarehouseId}",
                        "Inventory",
                        FinanceErrorCode.BusinessLogicError
                    );
                }

                var currentStock = warehouseStock.Quantity;

                _mapper.Map(request.Item, existingItem);
                existingItem.UpdatedAt = DateTime.UtcNow;
                existingItem.StockOnHand = currentStock;

                if (requisition.Type == RequisitionType.Inbound)
                {
                    existingItem.NewStockOnHand = currentStock + existingItem.Quantity;
                }
                else if (requisition.Type == RequisitionType.Outbound)
                {
                    existingItem.NewStockOnHand = currentStock - existingItem.Quantity;

                    if (existingItem.NewStockOnHand < 0)
                    {
                        throw new BusinessLogicException(
                            $"Insufficient stock. Available: {currentStock}, Requested: {existingItem.Quantity}",
                            "Inventory",
                            FinanceErrorCode.BusinessLogicError
                        );
                    }
                }

                existingItem.LineTotal = existingItem.UnitPrice.HasValue
                    ? existingItem.UnitPrice.Value * existingItem.Quantity
                    : (decimal?)null;

                await _repository.SaveChanges();

                _logger.Information("Item {ItemId} updated successfully", request.ItemId);

                var itemDto = await _dbContext.Set<RequisitionItem>()
                    .Where(x => x.Id == request.ItemId)
                    .ProjectTo<RequisitionItemDto>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync(cancellationToken);

                if (itemDto == null)
                {
                    throw new NotFoundException(
                        $"Item with ID {request.ItemId} not found after update",
                        FinanceErrorCode.NotFound
                    );
                }

                return ResponseViewModel<RequisitionItemDto>.Success(
                    itemDto,
                    "Requisition item updated successfully"
                );
            }
            catch (Exception ex) when (ex is not BusinessLogicException && ex is not NotFoundException)
            {
                _logger.Error(ex, "Error updating item {ItemId} for requisition {RequisitionId}",
                    request.ItemId, request.RequisitionId);
                throw;
            }
        }
    }
}