using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Enum.Inventory_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Requisitions.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.Requisitions.Commends.Commends_Requisition_Workflow
{
    public class RejectRequisitionHandler : IRequestHandler<RejectRequisitionCommand, ResponseViewModel<bool>>
    {
        private readonly IGeneralRepository<Requisition> _requisitionRepo;
        private readonly IGeneralRepository<RequisitionApprovalLog> _approvalLogRepo;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string MODULE_NAME = "Inventory";

        public RejectRequisitionHandler(
            IGeneralRepository<Requisition> requisitionRepo,
            IGeneralRepository<RequisitionApprovalLog> approvalLogRepo,
            IHttpContextAccessor httpContextAccessor)
        {
            _requisitionRepo = requisitionRepo;
            _approvalLogRepo = approvalLogRepo;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ResponseViewModel<bool>> Handle(RejectRequisitionCommand request, CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();

            var requisition = await _requisitionRepo
                .GetAll()
                .FirstOrDefaultAsync(r => r.Id == request.RequisitionId, cancellationToken);

            if (requisition == null)
            {
                throw new NotFoundException(
                    $"Requisition with ID {request.RequisitionId} not found",
                    FinanceErrorCode.NotFound);
            }

            if (requisition.Status != RequisitionStatus.Submitted)
            {
                throw new BusinessLogicException(
                    $"Only submitted requisitions can be rejected. Current status: {requisition.Status}",
                    MODULE_NAME,
                    FinanceErrorCode.BusinessLogicError);
            }

            requisition.Status = RequisitionStatus.Rejected;

            await _requisitionRepo.Update(requisition);

            var approvalLog = new RequisitionApprovalLog
            {
                RequisitionId = requisition.Id,
                Action = RequisitionAction.Reject,
                UserId = userId,
                Timestamp = DateTime.UtcNow,
                Comments = request.Comments
            };

            await _approvalLogRepo.AddAsync(approvalLog);
            await _approvalLogRepo.SaveChanges();

            return ResponseViewModel<bool>.Success(
                true,
                "Requisition rejected successfully");
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value
                ?? _httpContextAccessor.HttpContext?.User?.FindFirst("userId")?.Value;

            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }
    }
}