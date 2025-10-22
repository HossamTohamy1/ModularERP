using FluentValidation;
using ModularERP.Modules.Inventory.Features.Stocktaking.Commends.Commends_StockTaking_ImportExport;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Validators.Validators_StockTaking_ImportExport
{
    public class UploadStocktakingLineImageValidator : AbstractValidator<UploadStocktakingLineImageCommand>
    {
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
        private const long _maxFileSize = 5 * 1024 * 1024; // 5 MB

        public UploadStocktakingLineImageValidator()
        {
            RuleFor(x => x.StocktakingId)
                .NotEmpty()
                .WithMessage("Stocktaking ID is required");

            RuleFor(x => x.LineId)
                .NotEmpty()
                .WithMessage("Line ID is required");

            RuleFor(x => x.Image)
                .NotNull()
                .WithMessage("Image file is required")
                .Must(BeValidImageFile)
                .WithMessage($"Image must be one of: {string.Join(", ", _allowedExtensions)}")
                .Must(BeValidSize)
                .WithMessage($"Image size must not exceed {_maxFileSize / 1024 / 1024} MB");
        }

        private bool BeValidImageFile(IFormFile? file)
        {
            if (file == null) return false;

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            return _allowedExtensions.Contains(extension);
        }

        private bool BeValidSize(IFormFile? file)
        {
            if (file == null) return false;
            return file.Length <= _maxFileSize;
        }
    }
}