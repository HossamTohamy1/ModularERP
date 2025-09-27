namespace ModularERP.Modules.Finance.Features.ExpensesVoucher.DTO
{
    public class AttachmentResponseDto
    {
        public Guid Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public string MimeType { get; set; } = string.Empty;
        public int FileSize { get; set; }
        public string? Checksum { get; set; }
        public DateTime UploadedAt { get; set; }
        public Guid UploadedBy { get; set; }

        // Helper properties for UI
        public string FileSizeFormatted => FormatFileSize(FileSize);

        private static string FormatFileSize(int bytes)
        {
            if (bytes == 0) return "0 B";

            string[] sizes = { "B", "KB", "MB", "GB" };
            int order = 0;
            double size = bytes;

            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size /= 1024;
            }

            return $"{size:0.##} {sizes[order]}";
        }
    }
}