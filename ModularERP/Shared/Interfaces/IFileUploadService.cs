using ModularERP.Modules.Inventory.Features.Requisitions.DTO.DTO_Requisition;

namespace ModularERP.Shared.Interfaces
{
    public interface IFileUploadService
    {
        /// <summary>
        /// رفع ملف واحد
        /// </summary>
        /// <param name="file">الملف المراد رفعه</param>
        /// <param name="folderPath">مسار المجلد (اختياري)</param>
        /// <returns>معلومات الملف المرفوع</returns>
        Task<FileUploadResult> UploadFileAsync(IFormFile file, string? folderPath = null);

        /// <summary>
        /// رفع عدة ملفات
        /// </summary>
        /// <param name="files">قائمة الملفات</param>
        /// <param name="folderPath">مسار المجلد (اختياري)</param>
        /// <returns>قائمة بمعلومات الملفات المرفوعة</returns>
        Task<List<FileUploadResult>> UploadFilesAsync(List<IFormFile> files, string? folderPath = null);

        /// <summary>
        /// حذف ملف
        /// </summary>
        /// <param name="filePath">مسار الملف</param>
        Task<bool> DeleteFileAsync(string filePath);

        /// <summary>
        /// التحقق من صحة الملف
        /// </summary>
        /// <param name="file">الملف</param>
        /// <param name="allowedExtensions">الامتدادات المسموحة</param>
        /// <param name="maxFileSizeInMB">الحجم الأقصى بالميجابايت</param>
        /// <returns>نتيجة التحقق</returns>
        (bool isValid, string errorMessage) ValidateFile(
            IFormFile file,
            string[]? allowedExtensions = null,
            int maxFileSizeInMB = 10);
    }

}

