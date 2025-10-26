using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Commends.Commends_PurchaseOrder;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Models;
using ModularERP.Shared.Interfaces;
using Serilog;

namespace ModularERP.Modules.Purchases.Purchase_Order_Management.Handlers.Handlers_PurchaseOrder
{
    public class DeletePurchaseOrderHandler : IRequestHandler<DeletePurchaseOrderCommand, ResponseViewModel<bool>>
    {
        private readonly IGeneralRepository<PurchaseOrder> _poRepository;
        private readonly IGeneralRepository<POAttachment> _attachmentRepository;
        private readonly ILogger<DeletePurchaseOrderHandler> _logger;
        private readonly IWebHostEnvironment _environment;

        public DeletePurchaseOrderHandler(
            IGeneralRepository<PurchaseOrder> poRepository,
            IGeneralRepository<POAttachment> attachmentRepository,
            ILogger<DeletePurchaseOrderHandler> logger,
            IWebHostEnvironment environment)
        {
            _poRepository = poRepository;
            _attachmentRepository = attachmentRepository;
            _environment = environment;
            _logger = logger;
        }

        public async Task<ResponseViewModel<bool>> Handle(
            DeletePurchaseOrderCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Deleting purchase order with ID: {POId}", request.Id);

                // Get PO
                var purchaseOrder = await _poRepository.GetByIDWithTracking(request.Id);
                if (purchaseOrder == null)
                {
                    throw new NotFoundException(
                        $"Purchase order with ID {request.Id} not found",
                        FinanceErrorCode.NotFound);
                }

                // Validate status - only Draft can be deleted
                if (purchaseOrder.DocumentStatus != "Draft")
                {
                    throw new BusinessLogicException(
                        "Only purchase orders in Draft status can be deleted",
                        "PurchaseOrder");
                }

                // Check for related transactions
                var hasGoodsReceipts = await _poRepository
                    .GetAll()
                    .Where(x => x.Id == request.Id)
                    .Select(x => x.GoodsReceipts.Any())
                    .FirstOrDefaultAsync(cancellationToken);

                if (hasGoodsReceipts)
                {
                    throw new BusinessLogicException(
                        "Cannot delete purchase order with existing goods receipts",
                        "PurchaseOrder");
                }

                var hasInvoices = await _poRepository
                    .GetAll()
                    .Where(x => x.Id == request.Id)
                    .Select(x => x.Invoices.Any())
                    .FirstOrDefaultAsync(cancellationToken);

                if (hasInvoices)
                {
                    throw new BusinessLogicException(
                        "Cannot delete purchase order with existing invoices",
                        "PurchaseOrder");
                }

                // Delete attachments (physical files)
                var attachments = await _attachmentRepository
                    .Get(x => x.PurchaseOrderId == request.Id)
                    .ToListAsync(cancellationToken);

                foreach (var attachment in attachments)
                {
                    if (File.Exists(attachment.FilePath))
                    {
                        File.Delete(attachment.FilePath);
                    }
                }

                // Delete upload folder if empty
                var uploadPath = Path.Combine(_environment.WebRootPath, "uploads", "purchase-orders", request.Id.ToString());
                if (Directory.Exists(uploadPath) && !Directory.EnumerateFileSystemEntries(uploadPath).Any())
                {
                    Directory.Delete(uploadPath);
                }

                // Soft delete PO (cascade delete will handle related entities)
                await _poRepository.Delete(request.Id);

                _logger.LogInformation("Purchase order {PONumber} deleted successfully", purchaseOrder.PONumber);

                return ResponseViewModel<bool>.Success(true, "Purchase order deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting purchase order with ID: {POId}", request.Id);
                throw;
            }
        }
    }
}