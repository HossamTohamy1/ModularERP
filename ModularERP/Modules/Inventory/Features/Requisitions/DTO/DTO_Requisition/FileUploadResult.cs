namespace ModularERP.Modules.Inventory.Features.Requisitions.DTO.DTO_Requisition
{
    public class FileUploadResult
    {
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string MimeType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string? Checksum { get; set; }
    }
}
