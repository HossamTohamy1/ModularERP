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
    public class CreateRequisitionItemHandler : IRequestHandler<CreateRequisitionItemCommand, ResponseViewModel<RequisitionItemDto>>
    {
        private readonly IGeneralRepository<RequisitionItem> _repository;
        private readonly IGeneralRepository<Requisition> _requisitionRepository;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;
        private readonly FinanceDbContext _dbContext;

        public CreateRequisitionItemHandler(
            IGeneralRepository<RequisitionItem> repository,
            IGeneralRepository<Requisition> requisitionRepository,
            IMapper mapper,
            FinanceDbContext dbContext)
        {
            _repository = repository;
            _requisitionRepository = requisitionRepository;
            _mapper = mapper;
            _dbContext = dbContext;
            _logger = Log.ForContext<CreateRequisitionItemHandler>();
        }

        public async Task<ResponseViewModel<RequisitionItemDto>> Handle(CreateRequisitionItemCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.Information("Creating requisition item for requisition {RequisitionId}", request.RequisitionId);

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
                    _logger.Warning("Cannot add items to requisition {RequisitionId} with status {Status}",
                        request.RequisitionId, requisition.Status);
                    throw new BusinessLogicException(
                        "Cannot add items to a requisition that is not in Draft status",
                        "Inventory",
                        FinanceErrorCode.BusinessLogicError
                    );
                }

                var existingItem = await _repository.AnyAsync(
                    x => x.RequisitionId == request.RequisitionId &&
                         x.ProductId == request.Item.ProductId &&
                         !x.IsDeleted,
                    cancellationToken
                );

                if (existingItem)
                {
                    _logger.Warning("Product {ProductId} already exists in requisition {RequisitionId}",
                        request.Item.ProductId, request.RequisitionId);
                    throw new BusinessLogicException(
                        "This product already exists in the requisition",
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

                var currentStock = warehouseStock.Quantity ;

                var item = _mapper.Map<RequisitionItem>(request.Item);
                item.RequisitionId = request.RequisitionId;
                item.CreatedAt = DateTime.UtcNow;
                item.StockOnHand = currentStock;

                if (requisition.Type == RequisitionType.Inbound)
                {
                    item.NewStockOnHand = currentStock + item.Quantity;
                }
                else if (requisition.Type == RequisitionType.Outbound)
                {
                    item.NewStockOnHand = currentStock - item.Quantity;

                    if (item.NewStockOnHand < 0)
                    {
                        throw new BusinessLogicException(
                            $"Insufficient stock. Available: {currentStock}, Requested: {item.Quantity}",
                            "Inventory",
                            FinanceErrorCode.BusinessLogicError
                        );
                    }
                }

                item.LineTotal = item.UnitPrice.HasValue ? item.UnitPrice.Value * item.Quantity : (decimal?)null;

                await _repository.AddAsync(item);
                await _repository.SaveChanges();

                _logger.Information("Requisition item {ItemId} created successfully for requisition {RequisitionId}",
                    item.Id, request.RequisitionId);

                var itemDto = await _dbContext.Set<RequisitionItem>()
                    .Where(x => x.Id == item.Id)
                    .ProjectTo<RequisitionItemDto>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync(cancellationToken);

                if (itemDto == null)
                {
                    throw new NotFoundException(
                        $"Item with ID {item.Id} not found after creation",
                        FinanceErrorCode.NotFound
                    );
                }

                if (itemDto.ProductId != warehouseStock.ProductId)
                {
                    _logger.Warning("Mismatch between created item product and warehouse stock product. Item: {ItemProductId}, WarehouseStock: {WarehouseProductId}",
                        itemDto.ProductId, warehouseStock.ProductId);
                    throw new BusinessLogicException(
                        "Product mismatch between requisition item and warehouse stock",
                        "Inventory",
                        FinanceErrorCode.BusinessLogicError
                    );
                }

                return ResponseViewModel<RequisitionItemDto>.Success(
                    itemDto,
                    "Requisition item created successfully"
                );
            }
            catch (Exception ex) when (ex is not BusinessLogicException && ex is not NotFoundException)
            {
                _logger.Error(ex, "Error creating requisition item for requisition {RequisitionId}", request.RequisitionId);
                throw;
            }
        }

    }
}