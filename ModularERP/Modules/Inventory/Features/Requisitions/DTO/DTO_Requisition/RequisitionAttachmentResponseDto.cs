namespace ModularERP.Modules.Inventory.Features.Requisitions.DTO.DTO_Requisition
{
    public class RequisitionAttachmentResponseDto
    {
        public Guid Id { get; set; }
        public string Filename { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string MimeType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime UploadedAt { get; set; }
        public string UploadedByName { get; set; } = string.Empty;
    }
}
