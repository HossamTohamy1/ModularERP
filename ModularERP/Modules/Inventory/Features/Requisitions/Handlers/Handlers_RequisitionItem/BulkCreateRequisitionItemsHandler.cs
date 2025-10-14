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
    public class BulkCreateRequisitionItemsHandler : IRequestHandler<BulkCreateRequisitionItemsCommand, ResponseViewModel<List<RequisitionItemDto>>>
    {
        private readonly IGeneralRepository<RequisitionItem> _repository;
        private readonly IGeneralRepository<Requisition> _requisitionRepository;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;
        private readonly FinanceDbContext _dbContext;

        public BulkCreateRequisitionItemsHandler(
            IGeneralRepository<RequisitionItem> repository,
            IGeneralRepository<Requisition> requisitionRepository,
            IMapper mapper,
            FinanceDbContext dbContext)
        {
            _repository = repository;
            _requisitionRepository = requisitionRepository;
            _mapper = mapper;
            _dbContext = dbContext;
            _logger = Log.ForContext<BulkCreateRequisitionItemsHandler>();
        }

        public async Task<ResponseViewModel<List<RequisitionItemDto>>> Handle(BulkCreateRequisitionItemsCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.Information("Bulk creating {Count} items for requisition {RequisitionId}",
                    request.BulkItems.Items.Count, request.RequisitionId);

                // التحقق من وجود الـ Requisition
                var requisition = await _requisitionRepository.GetByID(request.RequisitionId);
                if (requisition == null)
                {
                    _logger.Warning("Requisition {RequisitionId} not found", request.RequisitionId);
                    throw new NotFoundException(
                        $"Requisition with ID {request.RequisitionId} not found",
                        FinanceErrorCode.NotFound
                    );
                }

                // التحقق من حالة الـ Requisition
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

                // جلب المنتجات الموجودة مسبقاً
                var existingProductIds = await _repository.Get(x => x.RequisitionId == request.RequisitionId && !x.IsDeleted)
                    .Select(x => x.ProductId)
                    .ToListAsync(cancellationToken);

                // التحقق من التكرارات في الـ Request
                var newProductIds = request.BulkItems.Items.Select(x => x.ProductId).ToList();
                var duplicates = newProductIds.GroupBy(x => x)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();

                if (duplicates.Any())
                {
                    _logger.Warning("Duplicate products found in bulk create request: {Duplicates}",
                        string.Join(", ", duplicates));
                    throw new BusinessLogicException(
                        $"Duplicate products found in the request: {string.Join(", ", duplicates)}",
                        "Inventory",
                        FinanceErrorCode.BusinessLogicError
                    );
                }

                // التحقق من المنتجات الموجودة مسبقاً
                var conflictingProducts = newProductIds.Intersect(existingProductIds).ToList();
                if (conflictingProducts.Any())
                {
                    _logger.Warning("Products already exist in requisition: {Products}",
                        string.Join(", ", conflictingProducts));
                    throw new BusinessLogicException(
                        $"The following products already exist in the requisition: {string.Join(", ", conflictingProducts)}",
                        "Inventory",
                        FinanceErrorCode.BusinessLogicError
                    );
                }

                // ✅ جلب معلومات المخزون لكل المنتجات دفعة واحدة
                var warehouseStocks = await _dbContext.WarehouseStocks
                    .Where(ws => ws.WarehouseId == requisition.WarehouseId && newProductIds.Contains(ws.ProductId))
                    .ToDictionaryAsync(ws => ws.ProductId, ws => ws.Quantity, cancellationToken);

                // التحقق من وجود كل المنتجات في المخزن
                var missingProducts = newProductIds.Except(warehouseStocks.Keys).ToList();
                if (missingProducts.Any())
                {
                    _logger.Warning("Products not found in warehouse {WarehouseId}: {Products}",
                        requisition.WarehouseId, string.Join(", ", missingProducts));
                    throw new BusinessLogicException(
                        $"The following products are not available in warehouse {requisition.WarehouseId}: {string.Join(", ", missingProducts)}",
                        "Inventory",
                        FinanceErrorCode.BusinessLogicError
                    );
                }

                // إنشاء العناصر
                var items = new List<RequisitionItem>();
                foreach (var itemDto in request.BulkItems.Items)
                {
                    var currentStock = warehouseStocks[itemDto.ProductId];

                    var item = _mapper.Map<RequisitionItem>(itemDto);
                    item.RequisitionId = request.RequisitionId;
                    item.CreatedAt = DateTime.UtcNow;
                    item.StockOnHand = currentStock;

                    // ✅ حساب NewStockOnHand بناءً على نوع Requisition
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
                                $"Insufficient stock for product {itemDto.ProductId}. Available: {currentStock}, Requested: {item.Quantity}",
                                "Inventory",
                                FinanceErrorCode.BusinessLogicError
                            );
                        }
                    }

                    item.LineTotal = item.UnitPrice.HasValue
                        ? item.UnitPrice.Value * item.Quantity
                        : (decimal?)null;

                    items.Add(item);
                }

                await _repository.AddRangeAsync(items);
                await _repository.SaveChanges();

                _logger.Information("Successfully created {Count} items for requisition {RequisitionId}",
                    items.Count, request.RequisitionId);

                // استخدام Projection لجلب البيانات المضافة
                var itemIds = items.Select(x => x.Id).ToList();
                var itemDtos = await _dbContext.Set<RequisitionItem>()
                    .Where(x => itemIds.Contains(x.Id))
                    .ProjectTo<RequisitionItemDto>(_mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);

                return ResponseViewModel<List<RequisitionItemDto>>.Success(
                    itemDtos,
                    $"Successfully created {itemDtos.Count} requisition items"
                );
            }
            catch (Exception ex) when (ex is not BusinessLogicException && ex is not NotFoundException)
            {
                _logger.Error(ex, "Error bulk creating items for requisition {RequisitionId}", request.RequisitionId);
                throw;
            }
        }
    }
}