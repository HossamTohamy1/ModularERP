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
    public class SubmitRequisitionHandler : IRequestHandler<SubmitRequisitionCommand, ResponseViewModel<bool>>
    {
        private readonly IGeneralRepository<Requisition> _requisitionRepo;
        private readonly IGeneralRepository<RequisitionApprovalLog> _approvalLogRepo;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string MODULE_NAME = "Inventory";

        public SubmitRequisitionHandler(
            IGeneralRepository<Requisition> requisitionRepo,
            IGeneralRepository<RequisitionApprovalLog> approvalLogRepo,
            IHttpContextAccessor httpContextAccessor)
        {
            _requisitionRepo = requisitionRepo;
            _approvalLogRepo = approvalLogRepo;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ResponseViewModel<bool>> Handle(SubmitRequisitionCommand request, CancellationToken cancellationToken)
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

            if (requisition.Status != RequisitionStatus.Draft)
            {
                throw new BusinessLogicException(
                    $"Only draft requisitions can be submitted. Current status: {requisition.Status}",
                    MODULE_NAME,
                    FinanceErrorCode.BusinessLogicError);
            }

            if (!requisition.Items.Any())
            {
                throw new BusinessLogicException(
                    "Cannot submit requisition without items",
                    MODULE_NAME,
                    FinanceErrorCode.BusinessLogicError);
            }

            requisition.Status = RequisitionStatus.Submitted;
            requisition.SubmittedBy = userId;
            requisition.SubmittedAt = DateTime.UtcNow;

            await _requisitionRepo.Update(requisition);

            var approvalLog = new RequisitionApprovalLog
            {
                RequisitionId = requisition.Id,
                Action = RequisitionAction.Submit,
                UserId = userId,
                Timestamp = DateTime.UtcNow,
                Comments = request.Comments
            };

            await _approvalLogRepo.AddAsync(approvalLog);
            await _approvalLogRepo.SaveChanges();

            return ResponseViewModel<bool>.Success(
                true,
                "Requisition submitted successfully");
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value
                ?? _httpContextAccessor.HttpContext?.User?.FindFirst("userId")?.Value;

            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }
    }
}