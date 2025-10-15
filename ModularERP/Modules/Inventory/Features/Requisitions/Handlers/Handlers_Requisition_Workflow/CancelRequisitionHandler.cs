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
    public class CancelRequisitionHandler : IRequestHandler<CancelRequisitionCommand, ResponseViewModel<bool>>
    {
        private readonly IGeneralRepository<Requisition> _requisitionRepo;
        private readonly IGeneralRepository<RequisitionApprovalLog> _approvalLogRepo;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string MODULE_NAME = "Inventory";

        public CancelRequisitionHandler(
            IGeneralRepository<Requisition> requisitionRepo,
            IGeneralRepository<RequisitionApprovalLog> approvalLogRepo,
            IHttpContextAccessor httpContextAccessor)
        {
            _requisitionRepo = requisitionRepo;
            _approvalLogRepo = approvalLogRepo;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ResponseViewModel<bool>> Handle(CancelRequisitionCommand request, CancellationToken cancellationToken)
        {
            Guid userId = Guid.Parse("f0602c31-0c12-4b5c-9ccf-fe17811d5c53");

            var requisition = await _requisitionRepo
                .GetAll()
                .FirstOrDefaultAsync(r => r.Id == request.RequisitionId, cancellationToken);

            if (requisition == null)
            {
                throw new NotFoundException(
                    $"Requisition with ID {request.RequisitionId} not found",
                    FinanceErrorCode.NotFound);
            }

            if (requisition.Status == RequisitionStatus.Confirmed)
            {
                throw new BusinessLogicException(
                    "Confirmed requisitions cannot be cancelled. Use reverse instead.",
                    MODULE_NAME,
                    FinanceErrorCode.BusinessLogicError);
            }

            if (requisition.Status == RequisitionStatus.Cancelled || requisition.Status == RequisitionStatus.Rejected)
            {
                throw new BusinessLogicException(
                    $"Requisition is already {requisition.Status.ToString().ToLower()}",
                    MODULE_NAME,
                    FinanceErrorCode.BusinessLogicError);
            }

            requisition.Status = RequisitionStatus.Cancelled;

            await _requisitionRepo.Update(requisition);

            var approvalLog = new RequisitionApprovalLog
            {
                RequisitionId = requisition.Id,
                Action = RequisitionAction.Cancel,
                UserId = userId,
                Timestamp = DateTime.UtcNow,
                Comments = request.Comments
            };

            await _approvalLogRepo.AddAsync(approvalLog);
            await _approvalLogRepo.SaveChanges();

            return ResponseViewModel<bool>.Success(
                true,
                "Requisition cancelled successfully");
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value
                ?? _httpContextAccessor.HttpContext?.User?.FindFirst("userId")?.Value;

            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }
    }
}