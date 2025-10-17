using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Requisitions.DTO.DTO_Requisition_Workflow;
using ModularERP.Modules.Inventory.Features.Requisitions.Models;
using ModularERP.Modules.Inventory.Features.Requisitions.Qeuries.Qeuier_ApprovalHistory;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.Requisitions.Handlers.Handlers_ApprovalHistory
{
    public class GetRequisitionApprovalLogHandler : IRequestHandler<GetRequisitionApprovalLogQuery, ResponseViewModel<List<ApprovalLogDto>>>
    {
        private readonly IGeneralRepository<Requisition> _requisitionRepo;
        private readonly IGeneralRepository<RequisitionApprovalLog> _approvalLogRepo;

        public GetRequisitionApprovalLogHandler(
            IGeneralRepository<Requisition> requisitionRepo,
            IGeneralRepository<RequisitionApprovalLog> approvalLogRepo)
        {
            _requisitionRepo = requisitionRepo;
            _approvalLogRepo = approvalLogRepo;
        }

        public async Task<ResponseViewModel<List<ApprovalLogDto>>> Handle(
            GetRequisitionApprovalLogQuery request,
            CancellationToken cancellationToken)
        {
            var requisitionExists = await _requisitionRepo
                .GetAll()
                .AnyAsync(r => r.Id == request.RequisitionId, cancellationToken);

            if (!requisitionExists)
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

            return ResponseViewModel<List<ApprovalLogDto>>.Success(
                approvalLogs,
                approvalLogs.Any()
                    ? "Approval logs retrieved successfully"
                    : "No approval logs found for this requisition");
        }
    }
}