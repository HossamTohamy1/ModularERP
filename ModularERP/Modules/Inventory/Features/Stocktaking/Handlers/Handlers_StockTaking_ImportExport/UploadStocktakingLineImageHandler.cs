using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Modules.Inventory.Features.Stocktaking.Commends.Commends_StockTaking_ImportExport;
using ModularERP.Modules.Inventory.Features.Stocktaking.DTO.DTO_StockTaking_ImportExport;
using ModularERP.Modules.Inventory.Features.Stocktaking.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Handlers.Handlers_StockTaking_ImportExport
{
    public class UploadStocktakingLineImageHandler : IRequestHandler<UploadStocktakingLineImageCommand, UploadImageResultDto>
    {
        private readonly IGeneralRepository<StocktakingLine> _lineRepo;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<UploadStocktakingLineImageHandler> _logger;
        private const string UploadFolder = "uploads/stocktaking";

        public UploadStocktakingLineImageHandler(
            IGeneralRepository<StocktakingLine> lineRepo,
            IWebHostEnvironment environment,
            ILogger<UploadStocktakingLineImageHandler> logger)
        {
            _lineRepo = lineRepo;
            _environment = environment;
            _logger = logger;
        }

        public async Task<UploadImageResultDto> Handle(
            UploadStocktakingLineImageCommand request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Uploading image for StocktakingId: {StocktakingId}, LineId: {LineId}",
                request.StocktakingId,
                request.LineId);

            var line = await _lineRepo
                .Get(l => l.Id == request.LineId &&
                         l.StocktakingId == request.StocktakingId &&
                         !l.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);

            if (line == null)
            {
                throw new NotFoundException(
                    "Stocktaking line not found",
                    FinanceErrorCode.NotFound);
            }

            // Delete old image if exists
            if (!string.IsNullOrEmpty(line.ImagePath))
            {
                DeleteOldImage(line.ImagePath);
            }

            // Generate unique filename
            var fileExtension = Path.GetExtension(request.Image.FileName);
            var uniqueFileName = $"{request.StocktakingId}_{request.LineId}_{Guid.NewGuid()}{fileExtension}";

            // Create upload directory if not exists
            var uploadPath = Path.Combine(_environment.WebRootPath, UploadFolder);
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            // Save file
            var filePath = Path.Combine(uploadPath, uniqueFileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await request.Image.CopyToAsync(stream, cancellationToken);
            }

            // Update database
            var relativePath = $"{UploadFolder}/{uniqueFileName}";
            line.ImagePath = relativePath;
            line.UpdatedAt = DateTime.UtcNow;

            await _lineRepo.SaveChanges();

            _logger.LogInformation(
                "Image uploaded successfully for LineId: {LineId}, Path: {Path}",
                request.LineId,
                relativePath);

            return new UploadImageResultDto
            {
                LineId = request.LineId,
                ImagePath = relativePath,
                ImageUrl = $"/{relativePath}",
                FileSize = request.Image.Length
            };
        }

        private void DeleteOldImage(string imagePath)
        {
            try
            {
                var fullPath = Path.Combine(_environment.WebRootPath, imagePath.TrimStart('/'));
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    _logger.LogInformation("Deleted old image: {Path}", imagePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete old image: {Path}", imagePath);
            }
        }
    }
}