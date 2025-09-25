using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.Attachments.Models;
using ModularERP.Modules.Finance.Features.ExpensesVoucher.Commands;
using ModularERP.Modules.Finance.Features.ExpensesVoucher.DTO;
using ModularERP.Modules.Finance.Features.ExpensesVoucher.Service;
using ModularERP.Modules.Finance.Features.Vouchers.Models;
using ModularERP.Modules.Finance.Features.VoucherTaxs.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Finance.Features.ExpensesVoucher.Handlers
{
    public class UpdateExpenseVoucherHandler : IRequestHandler<UpdateExpenseVoucherCommand, ResponseViewModel<ExpenseVoucherResponseDto>>
    {
        private readonly IGeneralRepository<Voucher> _voucherRepo;
        private readonly IGeneralRepository<VoucherTax> _voucherTaxRepo;
        private readonly IGeneralRepository<VoucherAttachment> _attachmentRepo;
        private readonly IExpenseVoucherService _expenseService;
        private readonly IMapper _mapper;
        private readonly IValidator<UpdateExpenseVoucherDto> _validator;
        private readonly ILogger<UpdateExpenseVoucherHandler> _logger;

        public UpdateExpenseVoucherHandler(
            IGeneralRepository<Voucher> voucherRepo,
            IGeneralRepository<VoucherTax> voucherTaxRepo,
            IGeneralRepository<VoucherAttachment> attachmentRepo,
            IExpenseVoucherService expenseService,
            IMapper mapper,
            IValidator<UpdateExpenseVoucherDto> validator,
            ILogger<UpdateExpenseVoucherHandler> logger)
        {
            _voucherRepo = voucherRepo;
            _voucherTaxRepo = voucherTaxRepo;
            _attachmentRepo = attachmentRepo;
            _expenseService = expenseService;
            _mapper = mapper;
            _validator = validator;
            _logger = logger;
        }

        public async Task<ResponseViewModel<ExpenseVoucherResponseDto>> Handle(UpdateExpenseVoucherCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var dto = request.Request;

                _logger.LogInformation("Start updating expense voucher {VoucherId}", dto.Id);

                // ✅ UserId ثابت
                var staticUserId = Guid.Parse("f0602c31-0c12-4b5c-9ccf-fe17811d5c53");

                // Validate DTO
                var validationResult = await _validator.ValidateAsync(dto, cancellationToken);
                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

                    _logger.LogWarning("Validation failed: {@Errors}", errors);
                    return ResponseViewModel<ExpenseVoucherResponseDto>.ValidationError("Validation failed", errors);
                }

                // Get existing voucher
                var existingVoucher = await _voucherRepo.GetByIDWithTracking(dto.Id);
                if (existingVoucher == null)
                {
                    return ResponseViewModel<ExpenseVoucherResponseDto>.Error(
                        "Expense voucher not found", FinanceErrorCode.NotFound);
                }

                if (existingVoucher.Type != VoucherType.Expense)
                {
                    return ResponseViewModel<ExpenseVoucherResponseDto>.Error(
                        "Voucher is not an expense voucher", FinanceErrorCode.BusinessLogicError);
                }

                if (existingVoucher.Status == VoucherStatus.Posted)
                {
                    return ResponseViewModel<ExpenseVoucherResponseDto>.Error(
                        "Cannot update posted voucher", FinanceErrorCode.BusinessLogicError);
                }

                // Business validations
                if (!await _expenseService.IsWalletActiveAsync(dto.Source.Id, dto.Source.Type))
                {
                    return ResponseViewModel<ExpenseVoucherResponseDto>.Error(
                        "Selected wallet is not active", FinanceErrorCode.BusinessLogicError);
                }

                if (!await _expenseService.IsCategoryValidAsync(dto.CategoryId))
                {
                    return ResponseViewModel<ExpenseVoucherResponseDto>.Error(
                        "Invalid expense category", FinanceErrorCode.BusinessLogicError);
                }

                if (!await _expenseService.IsDateInOpenPeriodAsync(dto.Date))
                {
                    return ResponseViewModel<ExpenseVoucherResponseDto>.Error(
                        "Date is not within open fiscal period", FinanceErrorCode.BusinessLogicError);
                }

                // Update voucher fields
                existingVoucher.Code = dto.Code ?? existingVoucher.Code;
                existingVoucher.Date = dto.Date;
                existingVoucher.Amount = dto.Amount;
                existingVoucher.CurrencyCode = dto.CurrencyCode;
                existingVoucher.FxRate = dto.FxRate;
                existingVoucher.Description = dto.Description;
                existingVoucher.CategoryAccountId = dto.CategoryId;
                existingVoucher.RecurrenceId = dto.RecurrenceId;
                existingVoucher.WalletType = Enum.Parse<WalletType>(dto.Source.Type);
                existingVoucher.WalletId = dto.Source.Id;
                existingVoucher.CounterpartyType = dto.Counterparty != null ? Enum.Parse<CounterpartyType>(dto.Counterparty.Type) : null;
                existingVoucher.CounterpartyId = dto.Counterparty?.Id;
                existingVoucher.JournalAccountId = await _expenseService.GetWalletControlAccountIdAsync(dto.Source.Id, dto.Source.Type);
                existingVoucher.UpdatedAt = DateTime.UtcNow;
                existingVoucher.CreatedBy = staticUserId; // ✅ UserId ثابت

                await _voucherRepo.SaveChanges();

                // Update tax lines
                var existingTaxes = await _voucherTaxRepo
                    .Get(t => t.VoucherId == dto.Id)
                    .ToListAsync(cancellationToken);

                foreach (var tax in existingTaxes)
                {
                    await _voucherTaxRepo.Delete(tax.Id);
                }

                var newTaxes = new List<VoucherTax>();
                if (dto.TaxLines?.Any() == true)
                {
                    foreach (var taxLineDto in dto.TaxLines)
                    {
                        newTaxes.Add(new VoucherTax
                        {
                            VoucherId = existingVoucher.Id,
                            TaxId = taxLineDto.TaxId,
                            BaseAmount = taxLineDto.BaseAmount,
                            TaxAmount = taxLineDto.TaxAmount,
                            IsWithholding = taxLineDto.IsWithholding,
                            Direction = TaxDirection.Expense
                        });
                    }

                    await _voucherTaxRepo.AddRangeAsync(newTaxes);
                    await _voucherTaxRepo.SaveChanges();
                }

                // Update attachments
                var existingAttachments = await _attachmentRepo
                    .Get(a => a.VoucherId == dto.Id)
                    .ToListAsync(cancellationToken);

                var attachmentsToRemove = existingAttachments
                    .Where(a => !dto.KeepAttachmentIds.Contains(a.Id))
                    .ToList();

                foreach (var attachment in attachmentsToRemove)
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", attachment.FilePath.TrimStart('/'));
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }

                    await _attachmentRepo.Delete(attachment.Id);
                }

                var newAttachments = new List<VoucherAttachment>();
                if (dto.NewAttachments?.Any() == true)
                {
                    var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                    if (!Directory.Exists(uploadDir))
                        Directory.CreateDirectory(uploadDir);

                    foreach (var file in dto.NewAttachments)
                    {
                        var fileName = $"{Guid.NewGuid()}_{file.FileName}";
                        var filePath = Path.Combine(uploadDir, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream, cancellationToken);
                        }

                        newAttachments.Add(new VoucherAttachment
                        {
                            VoucherId = existingVoucher.Id,
                            FilePath = $"/uploads/{fileName}",
                            Filename = file.FileName,
                            UploadedBy = staticUserId, // ✅ UserId ثابت
                            UploadedAt = DateTime.UtcNow
                        });
                    }

                    await _attachmentRepo.AddRangeAsync(newAttachments);
                    await _attachmentRepo.SaveChanges();
                }

                // Build response
                var response = _mapper.Map<ExpenseVoucherResponseDto>(existingVoucher);
                response.Source = dto.Source;
                response.Counterparty = dto.Counterparty;
                response.TaxLines = newTaxes.Select(t => new TaxLineResponseDto
                {
                    TaxId = t.TaxId,
                    BaseAmount = t.BaseAmount,
                    TaxAmount = t.TaxAmount,
                    IsWithholding = t.IsWithholding
                }).ToList();

                var allAttachments = existingAttachments
                    .Where(a => dto.KeepAttachmentIds.Contains(a.Id))
                    .Concat(newAttachments)
                    .ToList();

                response.Attachments = allAttachments.Select(a => new AttachmentResponseDto
                {
                    Id = a.Id,
                    FileName = a.Filename,
                    FileUrl = a.FilePath
                }).ToList();

                _logger.LogInformation("Successfully updated expense voucher {VoucherId}", dto.Id);

                return ResponseViewModel<ExpenseVoucherResponseDto>.Success(response, "Expense voucher updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating expense voucher {VoucherId}", request.Request.Id);
                return ResponseViewModel<ExpenseVoucherResponseDto>.Error(
                    "An error occurred while updating the expense voucher", FinanceErrorCode.InternalServerError);
            }
        }

    }
}