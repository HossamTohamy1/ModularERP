namespace ModularERP.Modules.Inventory.Features.ProductSettings.DTO
{
    public class CategoryAttachmentDto
    {
        public Guid Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string? FileType { get; set; }
        public long FileSize { get; set; }
        public string FileSizeFormatted { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public Guid? UploadedBy { get; set; }
        public string? UploadedByName { get; set; }
    }
}
