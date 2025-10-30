using MediatR;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Commends.Commends_POAttachment;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Purchase_Order_Management.Handlers.Handlers_POAttachment
{
    public class DeletePOAttachmentCommandHandler : IRequestHandler<DeletePOAttachmentCommand, ResponseViewModel<bool>>
    {
        private readonly IGeneralRepository<POAttachment> _attachmentRepository;
        private readonly ILogger<DeletePOAttachmentCommandHandler> _logger;

        public DeletePOAttachmentCommandHandler(
            IGeneralRepository<POAttachment> attachmentRepository,
            ILogger<DeletePOAttachmentCommandHandler> logger)
        {
            _attachmentRepository = attachmentRepository;
            _logger = logger;
        }

        public async Task<ResponseViewModel<bool>> Handle(DeletePOAttachmentCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting attachment {AttachmentId} for Purchase Order {PurchaseOrderId}", request.Id, request.PurchaseOrderId);

            var attachment = await _attachmentRepository.GetByID(request.Id);
            if (attachment == null || attachment.PurchaseOrderId != request.PurchaseOrderId)
            {
                throw new NotFoundException($"Attachment with ID {request.Id} not found for the specified Purchase Order", FinanceErrorCode.NotFound);
            }

            // Delete physical file
            var physicalPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", attachment.FilePath.TrimStart('/'));
            if (File.Exists(physicalPath))
            {
                try
                {
                    File.Delete(physicalPath);
                    _logger.LogInformation("Physical file deleted: {FilePath}", physicalPath);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete physical file: {FilePath}", physicalPath);
                }
            }

            // Delete database record
            await _attachmentRepository.Delete(request.Id);

            _logger.LogInformation("Attachment {AttachmentId} deleted successfully", request.Id);

            return ResponseViewModel<bool>.Success(true, "Attachment deleted successfully");
        }
    }
}
