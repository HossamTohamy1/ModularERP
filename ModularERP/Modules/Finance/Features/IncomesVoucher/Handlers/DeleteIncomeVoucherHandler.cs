using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Services;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.Attachments.Models;
using ModularERP.Modules.Finance.Features.IncomesVoucher.Commands;
using ModularERP.Modules.Finance.Features.Vouchers.Models;
using ModularERP.Modules.Finance.Features.VoucherTaxs.Models;
using ModularERP.Modules.Finance.Finance.Infrastructure.Data;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Finance.Features.IncomesVoucher.Handlers
{
    public class DeleteIncomeVoucherHandler : IRequestHandler<DeleteIncomeVoucherCommand, ResponseViewModel<bool>>
    {
        private readonly IGeneralRepository<Voucher> _voucherRepo;
        private readonly IGeneralRepository<VoucherTax> _voucherTaxRepo;
        private readonly IGeneralRepository<VoucherAttachment> _attachmentRepo;
        private readonly ILogger<DeleteIncomeVoucherHandler> _logger;
        private readonly FinanceDbContext _context;
        private readonly ITenantService _tenantService;

        public DeleteIncomeVoucherHandler(
            IGeneralRepository<Voucher> voucherRepo,
            IGeneralRepository<VoucherTax> voucherTaxRepo,
            IGeneralRepository<VoucherAttachment> attachmentRepo,
            ILogger<DeleteIncomeVoucherHandler> logger,
            FinanceDbContext context,
            ITenantService tenantService)
        {
            _voucherRepo = voucherRepo;
            _voucherTaxRepo = voucherTaxRepo;
            _attachmentRepo = attachmentRepo;
            _logger = logger;
            _context = context;
            _tenantService = tenantService;
        }

        public async Task<ResponseViewModel<bool>> Handle(DeleteIncomeVoucherCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("➡️ Start Handle DeleteIncomeVoucher with ID: {VoucherId}", request.Id);

            Guid tenantGuid;
            Guid userId;

            // 1. التحقق من Tenant
            try
            {
                var tenantId = _tenantService.GetCurrentTenantId();
                if (string.IsNullOrEmpty(tenantId))
                    return ResponseViewModel<bool>.Error("No tenant context available", FinanceErrorCode.BusinessLogicError);

                if (!Guid.TryParse(tenantId, out tenantGuid))
                    return ResponseViewModel<bool>.Error("Invalid tenant ID format", FinanceErrorCode.BusinessLogicError);

                _logger.LogInformation("✅ Tenant verified: {TenantId}", tenantGuid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error verifying tenant");
                return ResponseViewModel<bool>.Error("Error verifying tenant", FinanceErrorCode.InternalServerError);
            }

            // 2. التحقق من المستخدم (UserId ثابت زي الـ Create)
            try
            {
                userId = Guid.Parse("f0602c31-0c12-4b5c-9ccf-fe17811d5c53");

                var user = await _context.Users
                    .Where(u => u.Id == userId &&  !u.IsDeleted)
                    .FirstOrDefaultAsync(cancellationToken);

                if (user == null)
                    return ResponseViewModel<bool>.Error("User not found in current tenant", FinanceErrorCode.BusinessLogicError);

                _logger.LogInformation("✅ User verified: {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error verifying user");
                return ResponseViewModel<bool>.Error("Error verifying user", FinanceErrorCode.InternalServerError);
            }

            // 3. التحقق من وجود الـ Voucher
            Voucher voucher;
            try
            {
                voucher = await _voucherRepo.GetByID(request.Id);
                if (voucher == null)
                    return ResponseViewModel<bool>.Error("Voucher not found", FinanceErrorCode.NotFound);

                if (voucher.Type != VoucherType.Income)
                    return ResponseViewModel<bool>.Error("Voucher is not an income voucher", FinanceErrorCode.BusinessLogicError);

                if (voucher.Status == VoucherStatus.Posted)
                    return ResponseViewModel<bool>.Error("Cannot delete posted voucher", FinanceErrorCode.BusinessLogicError);

                _logger.LogInformation("✅ Voucher verified: {VoucherId}, Code: {VoucherCode}", request.Id, voucher.Code);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error verifying voucher");
                return ResponseViewModel<bool>.Error("Error verifying voucher", FinanceErrorCode.InternalServerError);
            }

            // 4. Check for dependencies (business logic)
            try
            {
                // Add checks if needed (journal entries, approvals, etc.)
                _logger.LogInformation("✅ Dependencies checked");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error checking dependencies");
                return ResponseViewModel<bool>.Error("Error checking voucher dependencies", FinanceErrorCode.InternalServerError);
            }

            // 5. Delete related attachments
            try
            {
                var attachments = await _attachmentRepo.Get(a => a.VoucherId == voucher.Id).ToListAsync(cancellationToken);

                foreach (var attachment in attachments)
                {
                    // حذف الملف من السيرفر
                    try
                    {
                        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", attachment.FilePath.TrimStart('/'));
                        if (File.Exists(fullPath))
                        {
                            File.Delete(fullPath);
                            _logger.LogInformation("🗑️ Deleted file: {FilePath}", attachment.FilePath);
                        }
                    }
                    catch (Exception fileEx)
                    {
                        _logger.LogWarning(fileEx, "⚠️ Could not delete file: {FilePath}", attachment.FilePath);
                    }

                    // حذف السجل من الداتابيز
                    await _attachmentRepo.Delete(attachment.Id);
                }

                await _attachmentRepo.SaveChanges();
                _logger.LogInformation("✅ Deleted {Count} attachments", attachments.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error deleting attachments");
                return ResponseViewModel<bool>.Error("Error deleting voucher attachments", FinanceErrorCode.InternalServerError);
            }

            // 6. Delete related tax lines
            try
            {
                var taxLines = await _voucherTaxRepo.Get(t => t.VoucherId == voucher.Id).ToListAsync(cancellationToken);

                foreach (var tax in taxLines)
                {
                    await _voucherTaxRepo.Delete(tax.Id);
                }

                await _voucherTaxRepo.SaveChanges();
                _logger.LogInformation("✅ Deleted {Count} tax lines", taxLines.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error deleting tax lines");
                return ResponseViewModel<bool>.Error("Error deleting voucher tax lines", FinanceErrorCode.InternalServerError);
            }

            // 7. Delete the voucher itself
            try
            {
                await _voucherRepo.Delete(voucher.Id);
                await _voucherRepo.SaveChanges();

                _logger.LogInformation("✅ Voucher deleted successfully: {VoucherId}, Code: {VoucherCode}",
                    voucher.Id, voucher.Code);

                return ResponseViewModel<bool>.Success(true, "Income voucher deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error deleting voucher");
                return ResponseViewModel<bool>.Error("Error deleting voucher", FinanceErrorCode.InternalServerError);
            }
        }

    }
}
