using FluentValidation;
using ModularERP.Modules.Inventory.Features.Stocktaking.Commends.Commends_StockTaking_ImportExport;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Validators.Validators_StockTaking_ImportExport
{
    public class ImportStocktakingValidator : AbstractValidator<ImportStocktakingCommand>
    {
        private readonly string[] _allowedExtensions = { ".xlsx", ".xls", ".csv" };
        private const long _maxFileSize = 10 * 1024 * 1024; // 10 MB

        public ImportStocktakingValidator()
        {
            RuleFor(x => x.StocktakingId)
                .NotEmpty()
                .WithMessage("Stocktaking ID is required");

            RuleFor(x => x.File)
                .NotNull()
                .WithMessage("File is required")
                .Must(BeValidFile)
                .WithMessage($"File must be one of: {string.Join(", ", _allowedExtensions)}")
                .Must(BeValidSize)
                .WithMessage($"File size must not exceed {_maxFileSize / 1024 / 1024} MB");
        }

        private bool BeValidFile(IFormFile? file)
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
