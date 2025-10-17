using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Enum.Inventory_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Requisitions.Commends.Commends_Requisition_Workflow;
using ModularERP.Modules.Inventory.Features.Requisitions.Models;
using ModularERP.Modules.Inventory.Features.Warehouses.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.Requisitions.Handlers.Handlers_Requisition_Workflow
{
    public class ConfirmRequisitionHandler : IRequestHandler<ConfirmRequisitionCommand, ResponseViewModel<bool>>
    {
        private readonly IGeneralRepository<Requisition> _requisitionRepo;
        private readonly IGeneralRepository<RequisitionItem> _requisitionItemRepo;
        private readonly IGeneralRepository<RequisitionApprovalLog> _approvalLogRepo;
        private readonly IGeneralRepository<WarehouseStock> _warehouseStockRepo;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string MODULE_NAME = "Inventory";

        public ConfirmRequisitionHandler(
            IGeneralRepository<Requisition> requisitionRepo,
            IGeneralRepository<RequisitionItem> requisitionItemRepo,
            IGeneralRepository<RequisitionApprovalLog> approvalLogRepo,
            IGeneralRepository<WarehouseStock> warehouseStockRepo,
            IHttpContextAccessor httpContextAccessor)
        {
            _requisitionRepo = requisitionRepo;
            _requisitionItemRepo = requisitionItemRepo;
            _approvalLogRepo = approvalLogRepo;
            _warehouseStockRepo = warehouseStockRepo;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ResponseViewModel<bool>> Handle(ConfirmRequisitionCommand request, CancellationToken cancellationToken)
        {
            Guid userId = Guid.Parse("f0602c31-0c12-4b5c-9ccf-fe17811d5c53");

            var requisition = await _requisitionRepo
                .GetAll()
                .Include(r => r.Items)
                .FirstOrDefaultAsync(r => r.Id == request.RequisitionId, cancellationToken);

            if (requisition == null)
            {
                throw new NotFoundException(
                    $"Requisition with ID {request.RequisitionId} not found",
                    FinanceErrorCode.NotFound);
            }

            if (requisition.Status != RequisitionStatus.Approved)
            {
                throw new BusinessLogicException(
                    $"Only approved requisitions can be confirmed. Current status: {requisition.Status}",
                    MODULE_NAME,
                    FinanceErrorCode.BusinessLogicError);
            }

            // تحديث المخزون لكل منتج
            foreach (var item in requisition.Items)
            {
                // جلب المخزون الحالي
                var warehouseStock = await _warehouseStockRepo
                    .GetAll()
                    .FirstOrDefaultAsync(ws => ws.WarehouseId == requisition.WarehouseId
                                            && ws.ProductId == item.ProductId,
                                         cancellationToken);

                if (warehouseStock == null)
                {
                    // إنشاء سجل جديد إذا لم يكن موجود
                    warehouseStock = new WarehouseStock
                    {
                        WarehouseId = requisition.WarehouseId,
                        ProductId = item.ProductId,
                        Quantity = 0,
                        AvailableQuantity = 0,
                        ReservedQuantity = 0,
                        LastStockInDate = null,
                        LastStockOutDate = null
                    };
                    await _warehouseStockRepo.AddAsync(warehouseStock);
                }

                decimal oldQuantity = warehouseStock.AvailableQuantity;
                decimal newQuantity;

                if (requisition.Type == RequisitionType.Inbound)
                {
                    // إضافة للمخزون
                    newQuantity = oldQuantity + item.Quantity;
                    warehouseStock.LastStockInDate = DateTime.UtcNow;

                    // تحديث متوسط التكلفة (Weighted Average Cost)
                    if (item.UnitPrice.HasValue && item.UnitPrice.Value > 0)
                    {
                        decimal oldValue = oldQuantity * (warehouseStock.AverageUnitCost ?? 0);
                        decimal newValue = item.Quantity * item.UnitPrice.Value;
                        warehouseStock.AverageUnitCost = (oldValue + newValue) / newQuantity;
                    }
                }
                else // Outbound
                {
                    // التحقق من وجود كمية كافية
                    if (oldQuantity < item.Quantity)
                    {
                        throw new BusinessLogicException(
                            $"Insufficient stock for product ID {item.ProductId}. Available: {oldQuantity}, Required: {item.Quantity}",
                            MODULE_NAME,
                            FinanceErrorCode.BusinessLogicError);
                    }

                    // خصم من المخزون
                    newQuantity = oldQuantity - item.Quantity;
                    warehouseStock.LastStockOutDate = DateTime.UtcNow;
                }

                // تحديث الكميات
                warehouseStock.Quantity = newQuantity;
                warehouseStock.AvailableQuantity = newQuantity - (warehouseStock.ReservedQuantity ?? 0);
                warehouseStock.TotalValue = newQuantity * (warehouseStock.AverageUnitCost ?? 0);
                warehouseStock.UpdatedAt = DateTime.UtcNow;

                await _warehouseStockRepo.Update(warehouseStock);

                // تحديث NewStockOnHand في RequisitionItem للعرض فقط
                item.StockOnHand = oldQuantity;
                item.NewStockOnHand = newQuantity;
                await _requisitionItemRepo.Update(item);
            }

            // تحديث حالة الـ Requisition
            requisition.Status = RequisitionStatus.Confirmed;
            requisition.ConfirmedBy = userId;
            requisition.ConfirmedAt = DateTime.UtcNow;

            await _requisitionRepo.Update(requisition);

            // تسجيل في الـ Approval Log
            var approvalLog = new RequisitionApprovalLog
            {
                RequisitionId = requisition.Id,
                Action = RequisitionAction.Confirm,
                UserId = userId,
                Timestamp = DateTime.UtcNow,
                Comments = request.Comments
            };

            await _approvalLogRepo.AddAsync(approvalLog);
            await _approvalLogRepo.SaveChanges();

            return ResponseViewModel<bool>.Success(
                true,
                "Requisition confirmed and stock updated successfully");
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value
                ?? _httpContextAccessor.HttpContext?.User?.FindFirst("userId")?.Value;

            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }
    }
}