namespace ModularERP.Modules.Inventory.Features.Stocktaking.Services
{
    public interface IFileStorageService
    {
        Task<string> SaveFileAsync(IFormFile file, string folder, CancellationToken cancellationToken = default);
        Task<bool> DeleteFileAsync(string filePath);
        Task<byte[]> ReadFileAsync(string filePath, CancellationToken cancellationToken = default);
        bool FileExists(string filePath);
    }

    public class FileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FileStorageService> _logger;

        public FileStorageService(
            IWebHostEnvironment environment,
            ILogger<FileStorageService> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        public async Task<string> SaveFileAsync(
            IFormFile file,
            string folder,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var uploadPath = Path.Combine(_environment.WebRootPath, folder);
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                var fileExtension = Path.GetExtension(file.FileName);
                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadPath, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream, cancellationToken);
                }

                var relativePath = $"{folder}/{uniqueFileName}";
                _logger.LogInformation("File saved successfully: {Path}", relativePath);

                return relativePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving file to {Folder}", folder);
                throw;
            }
        }

        public async Task<bool> DeleteFileAsync(string filePath)
        {
            try
            {
                var fullPath = Path.Combine(_environment.WebRootPath, filePath.TrimStart('/'));
                if (File.Exists(fullPath))
                {
                    await Task.Run(() => File.Delete(fullPath));
                    _logger.LogInformation("File deleted successfully: {Path}", filePath);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file: {Path}", filePath);
                return false;
            }
        }

        public async Task<byte[]> ReadFileAsync(string filePath, CancellationToken cancellationToken = default)
        {
            try
            {
                var fullPath = Path.Combine(_environment.WebRootPath, filePath.TrimStart('/'));
                if (!File.Exists(fullPath))
                {
                    throw new FileNotFoundException("File not found", filePath);
                }

                return await File.ReadAllBytesAsync(fullPath, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading file: {Path}", filePath);
                throw;
            }
        }

        public bool FileExists(string filePath)
        {
            try
            {
                var fullPath = Path.Combine(_environment.WebRootPath, filePath.TrimStart('/'));
                return File.Exists(fullPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking file existence: {Path}", filePath);
                return false;
            }
        }
    }
}