using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.Attachments.Models;
using ModularERP.Modules.Finance.Features.ExpensesVoucher.Commands;
using ModularERP.Modules.Finance.Features.Vouchers.Models;
using ModularERP.Modules.Finance.Features.VoucherTaxs.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Finance.Features.ExpensesVoucher.Handlers
{
    public class DeleteExpenseVoucherHandler : IRequestHandler<DeleteExpenseVoucherCommand, ResponseViewModel<string>>
    {
        private readonly IGeneralRepository<Voucher> _voucherRepo;
        private readonly IGeneralRepository<VoucherTax> _voucherTaxRepo;
        private readonly IGeneralRepository<VoucherAttachment> _attachmentRepo;
        private readonly ILogger<DeleteExpenseVoucherHandler> _logger;

        public DeleteExpenseVoucherHandler(
            IGeneralRepository<Voucher> voucherRepo,
            IGeneralRepository<VoucherTax> voucherTaxRepo,
            IGeneralRepository<VoucherAttachment> attachmentRepo,
            ILogger<DeleteExpenseVoucherHandler> logger)
        {
            _voucherRepo = voucherRepo;
            _voucherTaxRepo = voucherTaxRepo;
            _attachmentRepo = attachmentRepo;
            _logger = logger;
        }

        public async Task<ResponseViewModel<string>> Handle(DeleteExpenseVoucherCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Start deleting expense voucher {VoucherId}", request.Id);

                // Get existing voucher
                var existingVoucher = await _voucherRepo.GetByID(request.Id);
                if (existingVoucher == null)
                {
                    return ResponseViewModel<string>.Error(
                        "Expense voucher not found", FinanceErrorCode.NotFound);
                }

                if (existingVoucher.Type != VoucherType.Expense)
                {
                    return ResponseViewModel<string>.Error(
                        "Voucher is not an expense voucher", FinanceErrorCode.BusinessLogicError);
                }

                if (existingVoucher.Status == VoucherStatus.Posted)
                {
                    return ResponseViewModel<string>.Error(
                        "Cannot delete posted voucher", FinanceErrorCode.BusinessLogicError);
                }

                // Get and delete related tax lines
                var taxLines = await _voucherTaxRepo
                    .Get(t => t.VoucherId == request.Id)
                    .ToListAsync(cancellationToken);

                foreach (var tax in taxLines)
                {
                    await _voucherTaxRepo.Delete(tax.Id);
                }

                // Get and delete related attachments (including physical files)
                var attachments = await _attachmentRepo
                    .Get(a => a.VoucherId == request.Id)
                    .ToListAsync(cancellationToken);

                foreach (var attachment in attachments)
                {
                    // Delete physical file
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", attachment.FilePath.TrimStart('/'));
                    if (File.Exists(filePath))
                    {
                        try
                        {
                            File.Delete(filePath);
                            _logger.LogInformation("Deleted physical file: {FilePath}", filePath);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to delete physical file: {FilePath}", filePath);
                        }
                    }

                    await _attachmentRepo.Delete(attachment.Id);
                }

                // Delete the voucher (soft delete)
                await _voucherRepo.Delete(request.Id);

                _logger.LogInformation("Successfully deleted expense voucher {VoucherId} with {TaxCount} tax lines and {AttachmentCount} attachments",
                    request.Id, taxLines.Count, attachments.Count);

                return ResponseViewModel<string>.Success("success", "Expense voucher deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting expense voucher {VoucherId}", request.Id);
                return ResponseViewModel<string>.Error(
                    "An error occurred while deleting the expense voucher", FinanceErrorCode.InternalServerError);
            }
        }
    }
}
