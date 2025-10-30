namespace ModularERP.Modules.Purchases.Purchase_Order_Management.DTO.DTO_POAttachment
{
    public class POAttachmentResponseDto
    {
        public Guid Id { get; set; }
        public Guid PurchaseOrderId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime UploadedAt { get; set; }
        public string UploadedByName { get; set; } = string.Empty;
    }
}
