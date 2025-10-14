using ModularERP.Modules.Inventory.Features.Requisitions.DTO.DTO_Requisition;
using ModularERP.Shared.Interfaces;
using System.Security.Cryptography;

namespace ModularERP.Shared.Repository
{
    public class FileUploadService : IFileUploadService
    {
        private readonly string _uploadBasePath;
        private readonly string[] _defaultAllowedExtensions =
        {
            ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx",
            ".xls", ".xlsx", ".txt", ".zip", ".rar"
        };

        public FileUploadService(IWebHostEnvironment environment)
        {
            _uploadBasePath = Path.Combine(environment.ContentRootPath, "Uploads");

            // إنشاء المجلد إذا لم يكن موجوداً
            if (!Directory.Exists(_uploadBasePath))
            {
                Directory.CreateDirectory(_uploadBasePath);
            }
        }

        public async Task<FileUploadResult> UploadFileAsync(IFormFile file, string? folderPath = null)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is empty or null");
            }

            // تحديد مسار المجلد
            var uploadPath = string.IsNullOrEmpty(folderPath)
                ? _uploadBasePath
                : Path.Combine(_uploadBasePath, folderPath);

            // إنشاء المجلد إذا لم يكن موجوداً
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            // إنشاء اسم ملف فريد
            var fileExtension = Path.GetExtension(file.FileName);
            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadPath, uniqueFileName);

            // حفظ الملف
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // حساب الـ checksum
            var checksum = await CalculateChecksumAsync(filePath);

            return new FileUploadResult
            {
                FileName = file.FileName,
                FilePath = GetRelativePath(filePath),
                MimeType = file.ContentType,
                FileSize = file.Length,
                Checksum = checksum
            };
        }

        public async Task<List<FileUploadResult>> UploadFilesAsync(List<IFormFile> files, string? folderPath = null)
        {
            var results = new List<FileUploadResult>();

            foreach (var file in files)
            {
                var result = await UploadFileAsync(file, folderPath);
                results.Add(result);
            }

            return results;
        }

        public async Task<bool> DeleteFileAsync(string filePath)
        {
            try
            {
                var fullPath = Path.Combine(_uploadBasePath, filePath.TrimStart('/'));

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public (bool isValid, string errorMessage) ValidateFile(
            IFormFile file,
            string[]? allowedExtensions = null,
            int maxFileSizeInMB = 10)
        {
            if (file == null || file.Length == 0)
            {
                return (false, "File is empty");
            }

            // التحقق من الحجم
            var maxSizeInBytes = maxFileSizeInMB * 1024 * 1024;
            if (file.Length > maxSizeInBytes)
            {
                return (false, $"File size exceeds {maxFileSizeInMB}MB");
            }

            // التحقق من الامتداد
            var extensions = allowedExtensions ?? _defaultAllowedExtensions;
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!extensions.Contains(fileExtension))
            {
                return (false, $"File extension {fileExtension} is not allowed");
            }

            return (true, string.Empty);
        }

        private string GetRelativePath(string fullPath)
        {
            return fullPath.Replace(_uploadBasePath, "").Replace("\\", "/");
        }

        private async Task<string> CalculateChecksumAsync(string filePath)
        {
            using var md5 = MD5.Create();
            using var stream = File.OpenRead(filePath);
            var hash = await md5.ComputeHashAsync(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }
}