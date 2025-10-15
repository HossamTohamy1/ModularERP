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
    public class ReverseRequisitionHandler : IRequestHandler<ReverseRequisitionCommand, ResponseViewModel<Guid>>
    {
        private readonly IGeneralRepository<Requisition> _requisitionRepo;
        private readonly IGeneralRepository<RequisitionItem> _requisitionItemRepo;
        private readonly IGeneralRepository<RequisitionApprovalLog> _approvalLogRepo;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string MODULE_NAME = "Inventory";

        public ReverseRequisitionHandler(
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

        public async Task<ResponseViewModel<Guid>> Handle(ReverseRequisitionCommand request, CancellationToken cancellationToken)
        {
            Guid userId = Guid.Parse("f0602c31-0c12-4b5c-9ccf-fe17811d5c53");

            var originalRequisition = await _requisitionRepo
                .GetAll()
                .Include(r => r.Items)
                .FirstOrDefaultAsync(r => r.Id == request.RequisitionId, cancellationToken);

            if (originalRequisition == null)
            {
                throw new NotFoundException(
                    $"Requisition with ID {request.RequisitionId} not found",
                    FinanceErrorCode.NotFound);
            }

            if (originalRequisition.Status != RequisitionStatus.Confirmed)
            {
                throw new BusinessLogicException(
                    "Only confirmed requisitions can be reversed",
                    MODULE_NAME,
                    FinanceErrorCode.BusinessLogicError);
            }

            var reverseType = originalRequisition.Type == RequisitionType.Inbound
                ? RequisitionType.Outbound
                : RequisitionType.Inbound;

            var reverseRequisition = new Requisition
            {
                Type = reverseType,
                Date = DateTime.UtcNow,
                Number = await GenerateRequisitionNumber(cancellationToken),
                WarehouseId = originalRequisition.WarehouseId,
                JournalAccountId = originalRequisition.JournalAccountId,
                SupplierId = originalRequisition.SupplierId,
                Notes = $"Reversal of requisition {originalRequisition.Number}. Reason: {request.Comments}",
                Status = RequisitionStatus.Confirmed,
                CompanyId = originalRequisition.CompanyId,
                ParentRequisitionId = originalRequisition.Id,
                ConfirmedBy = userId,
                ConfirmedAt = DateTime.UtcNow
            };

            await _requisitionRepo.AddAsync(reverseRequisition);
            await _requisitionRepo.SaveChanges();

            foreach (var originalItem in originalRequisition.Items)
            {
                var reverseItem = new RequisitionItem
                {
                    RequisitionId = reverseRequisition.Id,
                    ProductId = originalItem.ProductId,
                    UnitPrice = originalItem.UnitPrice,
                    Quantity = originalItem.Quantity,
                    StockOnHand = originalItem.NewStockOnHand,
                    NewStockOnHand = originalItem.StockOnHand,
                    LineTotal = originalItem.LineTotal
                };

                await _requisitionItemRepo.AddAsync(reverseItem);
            }

            originalRequisition.ReversedBy = userId;
            originalRequisition.ReversedAt = DateTime.UtcNow;

            await _requisitionRepo.Update(originalRequisition);

            var approvalLog = new RequisitionApprovalLog
            {
                RequisitionId = originalRequisition.Id,
                Action = RequisitionAction.Reverse,
                UserId = userId,
                Timestamp = DateTime.UtcNow,
                Comments = request.Comments
            };

            await _approvalLogRepo.AddAsync(approvalLog);

            var reverseApprovalLog = new RequisitionApprovalLog
            {
                RequisitionId = reverseRequisition.Id,
                Action = RequisitionAction.Confirm,
                UserId = userId,
                Timestamp = DateTime.UtcNow,
                Comments = $"Auto-confirmed reversal of {originalRequisition.Number}"
            };

            await _approvalLogRepo.AddAsync(reverseApprovalLog);
            await _approvalLogRepo.SaveChanges();

            return ResponseViewModel<Guid>.Success(
                reverseRequisition.Id,
                $"Requisition reversed successfully. New requisition number: {reverseRequisition.Number}");
        }

        private async Task<string> GenerateRequisitionNumber(CancellationToken cancellationToken)
        {
            var lastRequisition = await _requisitionRepo
                .GetAll()
                .OrderByDescending(r => r.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            var lastNumber = lastRequisition?.Number ?? "REQ-0000";
            var numberPart = int.Parse(lastNumber.Replace("REQ-", ""));
            var newNumber = numberPart + 1;

            return $"REQ-{newNumber:D4}";
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value
                ?? _httpContextAccessor.HttpContext?.User?.FindFirst("userId")?.Value;

            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }
    }
}