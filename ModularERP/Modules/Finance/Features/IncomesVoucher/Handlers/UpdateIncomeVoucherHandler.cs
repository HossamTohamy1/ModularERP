using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Models;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.Attachments.Models;
using ModularERP.Modules.Finance.Features.IncomesVoucher.Commands;
using ModularERP.Modules.Finance.Features.IncomesVoucher.DTO;
using ModularERP.Modules.Finance.Features.IncomesVoucher.Service.Interface;
using ModularERP.Modules.Finance.Features.Vouchers.Models;
using ModularERP.Modules.Finance.Features.VoucherTaxs.Models;
using ModularERP.Modules.Finance.Finance.Infrastructure.Data;
using ModularERP.Shared.Interfaces;
using ModularERP.Common.Services;
using System.Text.Json;

namespace ModularERP.Modules.Finance.Features.IncomesVoucher.Handlers
{
    public class UpdateIncomeVoucherHandler : IRequestHandler<UpdateIncomeVoucherCommand, ResponseViewModel<IncomeVoucherResponseDto>>
    {
        private readonly IGeneralRepository<Voucher> _voucherRepo;
        private readonly IGeneralRepository<VoucherTax> _voucherTaxRepo;
        private readonly IGeneralRepository<VoucherAttachment> _attachmentRepo;
        private readonly IIncomeVoucherService _incomeService;
        private readonly IMapper _mapper;
        private readonly IValidator<UpdateIncomeVoucherDto> _validator;
        private readonly ILogger<UpdateIncomeVoucherHandler> _logger;
        private readonly FinanceDbContext _context;
        private readonly ITenantService _tenantService;

        public UpdateIncomeVoucherHandler(
            IGeneralRepository<Voucher> voucherRepo,
            IGeneralRepository<VoucherTax> voucherTaxRepo,
            IGeneralRepository<VoucherAttachment> attachmentRepo,
            IIncomeVoucherService incomeService,
            IMapper mapper,
            IValidator<UpdateIncomeVoucherDto> validator,
            ILogger<UpdateIncomeVoucherHandler> logger,
            FinanceDbContext context,
            ITenantService tenantService)
        {
            _voucherRepo = voucherRepo;
            _voucherTaxRepo = voucherTaxRepo;
            _attachmentRepo = attachmentRepo;
            _incomeService = incomeService;
            _mapper = mapper;
            _validator = validator;
            _logger = logger;
            _context = context;
            _tenantService = tenantService;
        }

        public async Task<ResponseViewModel<IncomeVoucherResponseDto>> Handle(UpdateIncomeVoucherCommand request, CancellationToken cancellationToken)
        {
            // 0. التهيئة
            var dto = request.Request;
            _logger.LogInformation("➡️ Start Handle UpdateIncomeVoucher with ID: {VoucherId}", dto.Id);

            Guid tenantGuid;
            Guid userId;
            Guid companyId;
            Voucher voucher = null;
            var taxes = new List<VoucherTax>();
            var attachments = new List<VoucherAttachment>();

            // 1. التحقق من Tenant
            try
            {
                var tenantId = _tenantService.GetCurrentTenantId();
                if (string.IsNullOrEmpty(tenantId))
                    return ResponseViewModel<IncomeVoucherResponseDto>.Error("No tenant context available", FinanceErrorCode.BusinessLogicError);

                if (!Guid.TryParse(tenantId, out tenantGuid))
                    return ResponseViewModel<IncomeVoucherResponseDto>.Error("Invalid tenant ID format", FinanceErrorCode.BusinessLogicError);

                _logger.LogInformation("✅ Tenant verified: {TenantId}", tenantGuid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error verifying tenant");
                return ResponseViewModel<IncomeVoucherResponseDto>.Error("Error verifying tenant", FinanceErrorCode.InternalServerError);
            }

            // 2. التحقق من المستخدم
            try
            {
                userId = Guid.Parse("f0602c31-0c12-4b5c-9ccf-fe17811d5c53");
                var user = await _context.Users
                    .Where(u => u.Id == userId && u.TenantId == tenantGuid && !u.IsDeleted)
                    .FirstOrDefaultAsync(cancellationToken);

                if (user == null)
                    return ResponseViewModel<IncomeVoucherResponseDto>.Error("User not found in current tenant", FinanceErrorCode.BusinessLogicError);

                _logger.LogInformation("✅ User verified: {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error verifying user");
                return ResponseViewModel<IncomeVoucherResponseDto>.Error("Error verifying user", FinanceErrorCode.InternalServerError);
            }

            // 3. التحقق من وجود الـ Voucher
            try
            {
                voucher = await _voucherRepo.GetByID(dto.Id);
                if (voucher == null)
                    return ResponseViewModel<IncomeVoucherResponseDto>.Error("Voucher not found", FinanceErrorCode.NotFound);

                if (voucher.Type != VoucherType.Income)
                    return ResponseViewModel<IncomeVoucherResponseDto>.Error("Voucher is not an income voucher", FinanceErrorCode.BusinessLogicError);

                if (voucher.Status == VoucherStatus.Posted)
                    return ResponseViewModel<IncomeVoucherResponseDto>.Error("Cannot update posted voucher", FinanceErrorCode.BusinessLogicError);

                companyId = voucher.CompanyId;

                // التحقق من الشركة
                var company = await _context.Companies
                    .Where(c => c.Id == companyId && c.TenantId == tenantGuid && !c.IsDeleted)
                    .FirstOrDefaultAsync(cancellationToken);

                if (company == null)
                    return ResponseViewModel<IncomeVoucherResponseDto>.Error("Company not found in current tenant", FinanceErrorCode.BusinessLogicError);

                _logger.LogInformation("✅ Voucher and Company verified: {VoucherId} - {CompanyId}", dto.Id, companyId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error verifying voucher");
                return ResponseViewModel<IncomeVoucherResponseDto>.Error("Error verifying voucher", FinanceErrorCode.InternalServerError);
            }

            // 4. Validate DTO
            try
            {
                var validationResult = await _validator.ValidateAsync(dto, cancellationToken);
                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

                    return ResponseViewModel<IncomeVoucherResponseDto>.ValidationError("Validation failed", errors);
                }
                _logger.LogInformation("✅ DTO validation passed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error validating DTO");
                return ResponseViewModel<IncomeVoucherResponseDto>.Error("Error validating request data", FinanceErrorCode.InternalServerError);
            }

            // 5. Business validations
            try
            {
                if (dto.SourceId.HasValue && !string.IsNullOrEmpty(dto.SourceType))
                {
                    if (!await _incomeService.IsWalletActiveAsync(dto.SourceId.Value, dto.SourceType))
                        return ResponseViewModel<IncomeVoucherResponseDto>.Error("Selected wallet is not active", FinanceErrorCode.BusinessLogicError);
                }

                if (!await _incomeService.IsCategoryValidAsync(dto.CategoryId))
                    return ResponseViewModel<IncomeVoucherResponseDto>.Error("Invalid revenue category", FinanceErrorCode.BusinessLogicError);

                if (!await _incomeService.IsDateInOpenPeriodAsync(dto.Date))
                    return ResponseViewModel<IncomeVoucherResponseDto>.Error("Date is not within open fiscal period", FinanceErrorCode.BusinessLogicError);

                _logger.LogInformation("✅ Business validations passed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in business validations");
                return ResponseViewModel<IncomeVoucherResponseDto>.Error("Error in business validations", FinanceErrorCode.InternalServerError);
            }

            // 6. Update voucher
            try
            {
                voucher.Date = dto.Date;
                voucher.Amount = dto.Amount;
                voucher.CurrencyCode = dto.CurrencyCode;
                voucher.FxRate = dto.FxRate;
                voucher.Description = dto.Description;
                voucher.CategoryAccountId = dto.CategoryId;
                voucher.RecurrenceId = dto.RecurrenceId;
                if (!string.IsNullOrEmpty(dto.Source?.Type) && Enum.TryParse<WalletType>(dto.Source.Type, out var walletType))
                {
                    voucher.WalletType = walletType;
                }
                else
                {
                    voucher.WalletType = WalletType.BankAccount; // default
                }
                voucher.WalletId = dto.Source?.Id ?? Guid.Empty;
                voucher.CreatedBy = userId;
                voucher.UpdatedAt = DateTime.UtcNow;

                // Update counterparty
                if (dto.Counterparty != null)
                {
                    voucher.CounterpartyId = dto.Counterparty.Id;
                    if (dto.Counterparty != null && Enum.TryParse<CounterpartyType>(dto.Counterparty.Type, out var counterpartyType))
                    {
                        voucher.CounterpartyId = dto.Counterparty.Id;
                        voucher.CounterpartyType = counterpartyType;
                    }
                    else
                    {
                        voucher.CounterpartyId = null;
                        voucher.CounterpartyType = null;
                    }
                }
                else
                {
                    voucher.CounterpartyId = null;
                    voucher.CounterpartyType = null;
                }

                if (dto.SourceId.HasValue && !string.IsNullOrEmpty(dto.SourceType))
                    voucher.JournalAccountId = await _incomeService.GetWalletControlAccountIdAsync(dto.SourceId.Value, dto.SourceType);

                await _voucherRepo.Update(voucher);
                await _voucherRepo.SaveChanges();

                _logger.LogInformation("✅ Voucher updated with ID: {VoucherId}", voucher.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error updating voucher");
                return ResponseViewModel<IncomeVoucherResponseDto>.Error("Error updating voucher", FinanceErrorCode.InternalServerError);
            }

            // 7. Update Tax Lines
            try
            {
                // Remove existing tax lines
                var existingTaxes = await _voucherTaxRepo.Get(t => t.VoucherId == voucher.Id).ToListAsync(cancellationToken);
                if (existingTaxes.Any())
                {
                    foreach (var tax in existingTaxes)
                    {
                        await _voucherTaxRepo.Delete(tax.Id);
                    }
                    await _voucherTaxRepo.SaveChanges();
                }

                // Add new tax lines
                if (!string.IsNullOrEmpty(dto.TaxLinesJson))
                {
                    var taxLinesFromJson = JsonSerializer.Deserialize<List<TaxLineDto>>(dto.TaxLinesJson);
                    if (taxLinesFromJson?.Any() == true)
                    {
                        foreach (var taxLineDto in taxLinesFromJson)
                        {
                            var voucherTax = new VoucherTax
                            {
                                VoucherId = voucher.Id,
                                TaxId = taxLineDto.TaxId,
                                BaseAmount = taxLineDto.BaseAmount,
                                TaxAmount = taxLineDto.TaxAmount,
                                IsWithholding = taxLineDto.IsWithholding,
                                TenantId = tenantGuid
                            };
                            taxes.Add(voucherTax);
                        }
                        await _voucherTaxRepo.AddRangeAsync(taxes);
                        await _voucherTaxRepo.SaveChanges();
                    }
                }
                _logger.LogInformation("✅ Tax lines updated");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error updating tax lines");
                return ResponseViewModel<IncomeVoucherResponseDto>.Error("Error updating tax lines", FinanceErrorCode.InternalServerError);
            }

            // 8. Update Attachments
            try
            {
                // Remove specified attachments
                if (dto.AttachmentsToRemove?.Any() == true)
                {
                    var attachmentsToRemove = await _attachmentRepo.Get(a => a.VoucherId == voucher.Id && dto.AttachmentsToRemove.Contains(a.Id))
                        .ToListAsync(cancellationToken);

                    foreach (var attachment in attachmentsToRemove)
                    {
                        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", attachment.FilePath.TrimStart('/'));
                        if (File.Exists(fullPath))
                        {
                            File.Delete(fullPath);
                            _logger.LogInformation("🗑️ Deleted file: {FilePath}", attachment.FilePath);
                        }

                        await _attachmentRepo.Delete(attachment.Id);
                    }
                    await _attachmentRepo.SaveChanges();
                    _logger.LogInformation("✅ Removed {Count} attachments", attachmentsToRemove.Count);
                }

                // Add new attachments with complete properties
                if (dto.NewAttachments?.Any() == true)
                {
                    var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                    if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

                    foreach (var file in dto.NewAttachments)
                    {
                        if (file.Length == 0)
                        {
                            _logger.LogWarning("Skipping empty file: {FileName}", file.FileName);
                            continue;
                        }

                        var fileName = $"{Guid.NewGuid()}_{file.FileName}";
                        var filePath = Path.Combine(uploadDir, fileName);
                        string checksum = null;

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        using (var sha256 = System.Security.Cryptography.SHA256.Create())
                        {
                            await file.CopyToAsync(stream, cancellationToken);
                            stream.Position = 0;
                            var hashBytes = await sha256.ComputeHashAsync(stream, cancellationToken);
                            checksum = Convert.ToHexString(hashBytes);
                        }

                        var mimeType = GetMimeType(file.FileName, file.ContentType);

                        var attachment = new VoucherAttachment
                        {
                            VoucherId = voucher.Id,
                            FilePath = $"/uploads/{fileName}",
                            Filename = file.FileName,
                            MimeType = mimeType,
                            FileSize = (int)file.Length,
                            Checksum = checksum,
                            UploadedBy = userId,
                            UploadedAt = DateTime.UtcNow,
                            TenantId = tenantGuid
                        };
                        attachments.Add(attachment);

                        _logger.LogInformation("✅ Processed new attachment: {FileName}, Size: {Size} bytes, MimeType: {MimeType}",
                            file.FileName, file.Length, mimeType);
                    }

                    if (attachments.Any())
                    {
                        await _attachmentRepo.AddRangeAsync(attachments);
                        await _attachmentRepo.SaveChanges();
                        _logger.LogInformation("✅ Added {Count} new attachments", attachments.Count);
                    }
                }
                _logger.LogInformation("✅ Attachments updated");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error updating attachments");
                return ResponseViewModel<IncomeVoucherResponseDto>.Error("Error updating attachments", FinanceErrorCode.InternalServerError);
            }

            // 9. Response
            try
            {
                // Get all current attachments
                var allAttachments = await _attachmentRepo.Get(a => a.VoucherId == voucher.Id).ToListAsync(cancellationToken);

                var response = _mapper.Map<IncomeVoucherResponseDto>(voucher);
                response.Source = dto.Source;
                response.Counterparty = dto.Counterparty;
                response.TaxLines = taxes.Select(t => new TaxLineResponseDto
                {
                    TaxId = t.TaxId,
                    BaseAmount = t.BaseAmount,
                    TaxAmount = t.TaxAmount,
                    IsWithholding = t.IsWithholding
                }).ToList();
                response.Attachments = _mapper.Map<List<AttachmentResponseDto>>(attachments);


                return ResponseViewModel<IncomeVoucherResponseDto>.Success(response, "Income voucher updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error creating response");
                return ResponseViewModel<IncomeVoucherResponseDto>.Error("Error creating response", FinanceErrorCode.InternalServerError);
            }
        }

        private string GetMimeType(string fileName, string contentType)
        {
            if (!string.IsNullOrEmpty(contentType) && contentType != "application/octet-stream")
                return contentType;

            var extension = Path.GetExtension(fileName)?.ToLowerInvariant();

            return extension switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".txt" => "text/plain",
                ".csv" => "text/csv",
                ".zip" => "application/zip",
                ".rar" => "application/x-rar-compressed",
                ".7z" => "application/x-7z-compressed",
                _ => "application/octet-stream"
            };
        }

    }
}