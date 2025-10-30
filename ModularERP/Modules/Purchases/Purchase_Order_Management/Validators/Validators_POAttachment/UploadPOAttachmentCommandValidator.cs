using FluentValidation;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Commends.Commends_POAttachment;

namespace ModularERP.Modules.Purchases.Purchase_Order_Management.Validators.Validators_POAttachment
{
    public class UploadPOAttachmentCommandValidator : AbstractValidator<UploadPOAttachmentCommand>
    {
        private readonly string[] _allowedExtensions = { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".jpg", ".jpeg", ".png", ".gif" };
        private const long MaxFileSize = 10 * 1024 * 1024; // 10MB

        public UploadPOAttachmentCommandValidator()
        {
            RuleFor(x => x.PurchaseOrderId)
                .NotEmpty().WithMessage("Purchase Order ID is required");

            RuleFor(x => x.File)
                .NotNull().WithMessage("File is required")
                .Must(file => file != null && file.Length > 0).WithMessage("File cannot be empty")
                .Must(file => file == null || file.Length <= MaxFileSize)
                    .WithMessage($"File size must not exceed {MaxFileSize / (1024 * 1024)}MB")
                .Must(file => file == null || _allowedExtensions.Contains(Path.GetExtension(file.FileName).ToLowerInvariant()))
                    .WithMessage($"Only the following file types are allowed: {string.Join(", ", _allowedExtensions)}");
        }
    }
}

