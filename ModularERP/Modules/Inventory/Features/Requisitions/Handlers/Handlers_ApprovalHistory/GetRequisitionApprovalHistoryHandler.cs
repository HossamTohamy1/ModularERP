using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Requisitions.DTO.DTO_ApprovalHistory;
using ModularERP.Modules.Inventory.Features.Requisitions.DTO.DTO_Requisition_Workflow;
using ModularERP.Modules.Inventory.Features.Requisitions.Models;
using ModularERP.Modules.Inventory.Features.Requisitions.Qeuries.Qeuier_ApprovalHistory;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.Requisitions.Handlers.Handlers_ApprovalHistory
{
    public class GetRequisitionApprovalHistoryHandler : IRequestHandler<GetRequisitionApprovalHistoryQuery, ResponseViewModel<ApprovalHistoryDto>>
    {
        private readonly IGeneralRepository<Requisition> _requisitionRepo;
        private readonly IGeneralRepository<RequisitionApprovalLog> _approvalLogRepo;

        public GetRequisitionApprovalHistoryHandler(
            IGeneralRepository<Requisition> requisitionRepo,
            IGeneralRepository<RequisitionApprovalLog> approvalLogRepo)
        {
            _requisitionRepo = requisitionRepo;
            _approvalLogRepo = approvalLogRepo;
        }

        public async Task<ResponseViewModel<ApprovalHistoryDto>> Handle(
            GetRequisitionApprovalHistoryQuery request,
            CancellationToken cancellationToken)
        {
            var requisition = await _requisitionRepo
                .GetAll()
                .Where(r => r.Id == request.RequisitionId)
                .Select(r => new
                {
                    r.Id,
                    r.Number,
                    r.Status,
                    r.Date,
                    r.CreatedById,
                    CreatedByName = r.CreatedByUser != null ? r.CreatedByUser.UserName : null,
                    r.CreatedAt,
                    r.SubmittedBy,
                    SubmittedByName = r.SubmittedByUser != null ? r.SubmittedByUser.UserName : null,
                    r.SubmittedAt,
                    r.ApprovedBy,
                    ApprovedByName = r.ApprovedByUser != null ? r.ApprovedByUser.UserName : null,
                    r.ApprovedAt,
                    r.ConfirmedBy,
                    ConfirmedByName = r.ConfirmedByUser != null ? r.ConfirmedByUser.UserName : null,
                    r.ConfirmedAt,
                    r.ReversedBy,
                    ReversedByName = r.ReversedByUser != null ? r.ReversedByUser.UserName : null,
                    r.ReversedAt
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (requisition == null)
            {
                throw new NotFoundException(
                    $"Requisition with ID {request.RequisitionId} not found",
                    FinanceErrorCode.NotFound);
            }

            var approvalLogs = await _approvalLogRepo
                .GetAll()
                .Where(log => log.RequisitionId == request.RequisitionId)
                .OrderBy(log => log.Timestamp)
                .Select(log => new ApprovalLogDto
                {
                    Id = log.Id,
                    Action = log.Action.ToString(),
                    UserId = log.UserId,
                    UserName = log.User != null ? log.User.UserName : null,
                    Timestamp = log.Timestamp,
                    Comments = log.Comments
                })
                .ToListAsync(cancellationToken);

            var history = new ApprovalHistoryDto
            {
                RequisitionId = requisition.Id,
                Number = requisition.Number,
                CurrentStatus = requisition.Status.ToString(),
                Date = requisition.Date,
                Timeline = approvalLogs,
                TotalActionsCount = approvalLogs.Count
            };

            history.Created = new WorkflowStepDto
            {
                Action = "Created",
                UserId = requisition.CreatedById,
                UserName = requisition.CreatedByName,
                Timestamp = requisition.CreatedAt
            };

            if (requisition.SubmittedAt.HasValue)
            {
                history.Submitted = new WorkflowStepDto
                {
                    Action = "Submitted",
                    UserId = requisition.SubmittedBy,
                    UserName = requisition.SubmittedByName,
                    Timestamp = requisition.SubmittedAt,
                    Comments = approvalLogs.FirstOrDefault(l => l.Action == "Submit")?.Comments
                };
            }

            if (requisition.ApprovedAt.HasValue)
            {
                history.Approved = new WorkflowStepDto
                {
                    Action = "Approved",
                    UserId = requisition.ApprovedBy,
                    UserName = requisition.ApprovedByName,
                    Timestamp = requisition.ApprovedAt,
                    Comments = approvalLogs.FirstOrDefault(l => l.Action == "Approve")?.Comments
                };
            }

            if (requisition.ConfirmedAt.HasValue)
            {
                history.Confirmed = new WorkflowStepDto
                {
                    Action = "Confirmed",
                    UserId = requisition.ConfirmedBy,
                    UserName = requisition.ConfirmedByName,
                    Timestamp = requisition.ConfirmedAt,
                    Comments = approvalLogs.FirstOrDefault(l => l.Action == "Confirm")?.Comments
                };
            }

            if (requisition.ReversedAt.HasValue)
            {
                history.Reversed = new WorkflowStepDto
                {
                    Action = "Reversed",
                    UserId = requisition.ReversedBy,
                    UserName = requisition.ReversedByName,
                    Timestamp = requisition.ReversedAt,
                    Comments = approvalLogs.FirstOrDefault(l => l.Action == "Reverse")?.Comments
                };
            }

            var rejectedLog = approvalLogs.FirstOrDefault(l => l.Action == "Reject");
            if (rejectedLog != null)
            {
                history.Rejected = new WorkflowStepDto
                {
                    Action = "Rejected",
                    UserId = rejectedLog.UserId,
                    UserName = rejectedLog.UserName,
                    Timestamp = rejectedLog.Timestamp,
                    Comments = rejectedLog.Comments
                };
            }

            var cancelledLog = approvalLogs.FirstOrDefault(l => l.Action == "Cancel");
            if (cancelledLog != null)
            {
                history.Cancelled = new WorkflowStepDto
                {
                    Action = "Cancelled",
                    UserId = cancelledLog.UserId,
                    UserName = cancelledLog.UserName,
                    Timestamp = cancelledLog.Timestamp,
                    Comments = cancelledLog.Comments
                };
            }

            // Fixing the CS1061 error by removing the incorrect 'HasValue' check for DateTime.
            if (requisition.CreatedAt != default && requisition.ConfirmedAt.HasValue)
            {
                history.ProcessingDuration = requisition.ConfirmedAt.Value - requisition.CreatedAt;
            }


            return ResponseViewModel<ApprovalHistoryDto>.Success(
                history,
                "Approval history retrieved successfully");
        }
    }
}