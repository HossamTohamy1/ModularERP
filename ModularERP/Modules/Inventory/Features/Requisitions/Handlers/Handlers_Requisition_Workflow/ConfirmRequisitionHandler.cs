using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Enum.Inventory_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Requisitions.Commends.Commends_Requisition_Workflow;
using ModularERP.Modules.Inventory.Features.Requisitions.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.Requisitions.Handlers.Handlers_Requisition_Workflow
{
    public class ConfirmRequisitionHandler : IRequestHandler<ConfirmRequisitionCommand, ResponseViewModel<bool>>
    {
        private readonly IGeneralRepository<Requisition> _requisitionRepo;
        private readonly IGeneralRepository<RequisitionItem> _requisitionItemRepo;
        private readonly IGeneralRepository<RequisitionApprovalLog> _approvalLogRepo;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string MODULE_NAME = "Inventory";

        public ConfirmRequisitionHandler(
            IGeneralRepository<Requisition> requisitionRepo,
            IGeneralRepository<RequisitionItem> requisitionItemRepo,
            IGeneralRepository<RequisitionApprovalLog> approvalLogRepo,
            IHttpContextAccessor httpContextAccessor)
        {
            _requisitionRepo = requisitionRepo;
            _requisitionItemRepo = requisitionItemRepo;
            _approvalLogRepo = approvalLogRepo;
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

            foreach (var item in requisition.Items)
            {
                item.NewStockOnHand = requisition.Type == RequisitionType.Inbound
                    ? (item.StockOnHand ?? 0) + item.Quantity
                    : (item.StockOnHand ?? 0) - item.Quantity;

                if (requisition.Type == RequisitionType.Outbound && item.NewStockOnHand < 0)
                {
                    throw new BusinessLogicException(
                        $"Insufficient stock for product ID {item.ProductId}. Available: {item.StockOnHand}, Required: {item.Quantity}",
                        MODULE_NAME,
                        FinanceErrorCode.BusinessLogicError);
                }

                await _requisitionItemRepo.Update(item);
            }

            requisition.Status = RequisitionStatus.Confirmed;
            requisition.ConfirmedBy = userId;
            requisition.ConfirmedAt = DateTime.UtcNow;

            await _requisitionRepo.Update(requisition);

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