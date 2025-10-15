namespace ModularERP.Modules.Inventory.Features.Requisitions.DTO.DTO_Requisition_Workflow
{
    public class WorkflowStatusDto
    {
        public Guid RequisitionId { get; set; }
        public string Number { get; set; } = string.Empty;
        public string CurrentStatus { get; set; } = string.Empty;
        public DateTime Date { get; set; }

        public WorkflowStepDto? Draft { get; set; }
        public WorkflowStepDto? Submitted { get; set; }
        public WorkflowStepDto? Approved { get; set; }
        public WorkflowStepDto? Confirmed { get; set; }
        public WorkflowStepDto? Rejected { get; set; }
        public WorkflowStepDto? Cancelled { get; set; }
        public WorkflowStepDto? Reversed { get; set; }
        public List<ApprovalLogDto> ApprovalLogs { get; set; } = new();

    }
}
