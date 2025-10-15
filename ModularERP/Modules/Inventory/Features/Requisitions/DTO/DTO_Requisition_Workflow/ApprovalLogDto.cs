namespace ModularERP.Modules.Inventory.Features.Requisitions.DTO.DTO_Requisition_Workflow
{
    public class ApprovalLogDto
    {
        public Guid Id { get; set; }
        public string Action { get; set; } = string.Empty;
        public Guid? UserId { get; set; }
        public string? UserName { get; set; }
        public DateTime Timestamp { get; set; }
        public string? Comments { get; set; }
    }
}
