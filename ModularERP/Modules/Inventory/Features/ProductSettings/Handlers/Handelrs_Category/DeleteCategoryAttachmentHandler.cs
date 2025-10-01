using MediatR;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.ProductSettings.Commends.Commends_Category;
using ModularERP.Modules.Inventory.Features.ProductSettings.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.Handlers.Handelrs_Category
{
    public class DeleteCategoryAttachmentHandler : IRequestHandler<DeleteCategoryAttachmentCommand, ResponseViewModel<bool>>
    {
        private readonly IGeneralRepository<CategoryAttachment> _attachmentRepo;

        public DeleteCategoryAttachmentHandler(IGeneralRepository<CategoryAttachment> attachmentRepo)
        {
            _attachmentRepo = attachmentRepo;
        }

        public async Task<ResponseViewModel<bool>> Handle(
            DeleteCategoryAttachmentCommand request,
            CancellationToken cancellationToken)
        {
            var attachment = await _attachmentRepo.GetByID(request.AttachmentId);

            if (attachment == null)
                throw new NotFoundException("Attachment not found", FinanceErrorCode.NotFound);

            if (attachment.CategoryId != request.CategoryId)
            {
                throw new BusinessLogicException(
                    "Attachment does not belong to this category",
                    "Inventory",
                    FinanceErrorCode.BusinessLogicError);
            }

            // Delete physical file
            DeletePhysicalFile(attachment.FilePath);

            // Delete from database
            await _attachmentRepo.Delete(request.AttachmentId);
            await _attachmentRepo.SaveChanges();

            return ResponseViewModel<bool>.Success(true, "Attachment deleted successfully");
        }

        private void DeletePhysicalFile(string filePath)
        {
            try
            {
                var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", filePath.TrimStart('/'));
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
            }
            catch (Exception)
            {
                // Log error but don't throw
            }
        }
    }
}


