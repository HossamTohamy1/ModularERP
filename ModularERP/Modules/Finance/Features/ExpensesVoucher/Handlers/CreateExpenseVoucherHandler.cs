using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Models;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.Attachments.Models;
using ModularERP.Modules.Finance.Features.ExpensesVoucher.Commands;
using ModularERP.Modules.Finance.Features.ExpensesVoucher.DTO;
using ModularERP.Modules.Finance.Features.ExpensesVoucher.Service;
using ModularERP.Modules.Finance.Features.Vouchers.Models;
using ModularERP.Modules.Finance.Features.VoucherTaxs.Models;
using ModularERP.Modules.Finance.Finance.Infrastructure.Data;
using ModularERP.Shared.Interfaces;
using ModularERP.Common.Services;
using System.Text.Json;
using System.Security.Cryptography;
using ModularERP.Modules.Finance.Features.ExpensesVoucher.DTO.ModularERP.Modules.Finance.Features.IncomesVoucher.DTO;

namespace ModularERP.Modules.Finance.Features.ExpensesVoucher.Handlers
{
    public class CreateExpenseVoucherHandler : IRequestHandler<CreateExpenseVoucherCommand, ResponseViewModel<ExpenseVoucherResponseDto>>
    {
        private readonly IGeneralRepository<Voucher> _voucherRepo;
        private readonly IGeneralRepository<VoucherTax> _voucherTaxRepo;
        private readonly IGeneralRepository<VoucherAttachment> _attachmentRepo;
        private readonly IExpenseVoucherService _expenseService;
        private readonly IMapper _mapper;
        private readonly IValidator<CreateExpenseVoucherDto> _validator;
        private readonly ILogger<CreateExpenseVoucherHandler> _logger;
        private readonly FinanceDbContext _context;
        private readonly ITenantService _tenantService;

        public CreateExpenseVoucherHandler(
            IGeneralRepository<Voucher> voucherRepo,
            IGeneralRepository<VoucherTax> voucherTaxRepo,
            IGeneralRepository<VoucherAttachment> attachmentRepo,
            IExpenseVoucherService expenseService,
            IMapper mapper,
            IValidator<CreateExpenseVoucherDto> validator,
            ILogger<CreateExpenseVoucherHandler> logger,
            FinanceDbContext context,
            ITenantService tenantService)
        {
            _voucherRepo = voucherRepo;
            _voucherTaxRepo = voucherTaxRepo;
            _attachmentRepo = attachmentRepo;
            _expenseService = expenseService;
            _mapper = mapper;
            _validator = validator;
            _logger = logger;
            _context = context;
            _tenantService = tenantService;
        }

        public async Task<ResponseViewModel<ExpenseVoucherResponseDto>> Handle(CreateExpenseVoucherCommand request, CancellationToken cancellationToken)
        {
            // 0. التهيئة
            var dto = request.Request;
            _logger.LogInformation("➡️ Start Handle CreateExpenseVoucher with Request: {@Dto}", dto);

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
                    return ResponseViewModel<ExpenseVoucherResponseDto>.Error("No tenant context available", FinanceErrorCode.BusinessLogicError);

                if (!Guid.TryParse(tenantId, out tenantGuid))
                    return ResponseViewModel<ExpenseVoucherResponseDto>.Error("Invalid tenant ID format", FinanceErrorCode.BusinessLogicError);

                _logger.LogInformation("✅ Tenant verified: {TenantId}", tenantGuid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error verifying tenant");
                return ResponseViewModel<ExpenseVoucherResponseDto>.Error("Error verifying tenant", FinanceErrorCode.InternalServerError);
            }

            // 2. التحقق من المستخدم
            try
            {
                userId = Guid.Parse("f0602c31-0c12-4b5c-9ccf-fe17811d5c53");
                var user = await _context.Users
                    .Where(u => u.Id == userId && u.TenantId == tenantGuid && !u.IsDeleted)
                    .FirstOrDefaultAsync(cancellationToken);

                if (user == null)
                    return ResponseViewModel<ExpenseVoucherResponseDto>.Error("User not found in current tenant", FinanceErrorCode.BusinessLogicError);

                _logger.LogInformation("✅ User verified: {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error verifying user");
                return ResponseViewModel<ExpenseVoucherResponseDto>.Error("Error verifying user", FinanceErrorCode.InternalServerError);
            }

            // 3. التحقق من الشركة
            try
            {
                companyId = Guid.Parse("82b0fb02-1282-483c-ae26-c8466423707e");
                var company = await _context.Companies
                    .Where(c => c.Id == companyId && c.TenantId == tenantGuid && !c.IsDeleted)
                    .FirstOrDefaultAsync(cancellationToken);

                if (company == null)
                    return ResponseViewModel<ExpenseVoucherResponseDto>.Error("Company not found in current tenant", FinanceErrorCode.BusinessLogicError);

                _logger.LogInformation("✅ Company verified: {CompanyId}", companyId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error verifying company");
                return ResponseViewModel<ExpenseVoucherResponseDto>.Error("Error verifying company", FinanceErrorCode.InternalServerError);
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

                    return ResponseViewModel<ExpenseVoucherResponseDto>.ValidationError("Validation failed", errors);
                }
                _logger.LogInformation("✅ DTO validation passed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error validating DTO");
                return ResponseViewModel<ExpenseVoucherResponseDto>.Error("Error validating request data", FinanceErrorCode.InternalServerError);
            }

            // 5. Business validations
            try
            {
                if (dto.SourceId.HasValue && !string.IsNullOrEmpty(dto.SourceType))
                {
                    if (!await _expenseService.IsWalletActiveAsync(dto.SourceId.Value, dto.SourceType))
                        return ResponseViewModel<ExpenseVoucherResponseDto>.Error("Selected wallet is not active", FinanceErrorCode.BusinessLogicError);
                }

                if (!await _expenseService.IsCategoryValidAsync(dto.CategoryId))
                    return ResponseViewModel<ExpenseVoucherResponseDto>.Error("Invalid expense category", FinanceErrorCode.BusinessLogicError);

                if (!await _expenseService.IsDateInOpenPeriodAsync(dto.Date))
                    return ResponseViewModel<ExpenseVoucherResponseDto>.Error("Date is not within open fiscal period", FinanceErrorCode.BusinessLogicError);

                _logger.LogInformation("✅ Business validations passed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in business validations");
                return ResponseViewModel<ExpenseVoucherResponseDto>.Error("Error in business validations", FinanceErrorCode.InternalServerError);
            }

            // 6. Generate code
            try
            {
                var code = string.IsNullOrEmpty(dto.Code)
                    ? await _expenseService.GenerateVoucherCodeAsync(VoucherType.Expense, dto.Date)
                    : dto.Code;

                if (await _voucherRepo.AnyAsync(v => v.CompanyId == companyId && v.Code == code))
                    return ResponseViewModel<ExpenseVoucherResponseDto>.Error($"Voucher code '{code}' already exists", FinanceErrorCode.BusinessLogicError);

                voucher = _mapper.Map<Voucher>(dto);
                voucher.Code = code;
                voucher.CompanyId = companyId;
                voucher.TenantId = tenantGuid;
                voucher.CreatedBy = userId;

                if (dto.SourceId.HasValue && !string.IsNullOrEmpty(dto.SourceType))
                    voucher.JournalAccountId = await _expenseService.GetWalletControlAccountIdAsync(dto.SourceId.Value, dto.SourceType);

                await _voucherRepo.AddAsync(voucher);
                await _voucherRepo.SaveChanges();

                _logger.LogInformation("✅ Voucher saved with ID: {VoucherId}", voucher.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error creating voucher");
                return ResponseViewModel<ExpenseVoucherResponseDto>.Error("Error creating voucher", FinanceErrorCode.InternalServerError);
            }

            // 7. Tax Lines
            try
            {
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
                                TaxProfileId = taxLineDto.TaxProfileId,
                                TaxComponentId = taxLineDto.TaxComponentId,
                                BaseAmount = taxLineDto.BaseAmount,
                                TaxAmount = taxLineDto.TaxAmount,
                                AppliedRate = taxLineDto.AppliedRate,
                                IsWithholding = taxLineDto.IsWithholding,
                                Direction = TaxDirection.Expense,
                                TenantId = tenantGuid
                            };
                            taxes.Add(voucherTax);
                        }
                        await _voucherTaxRepo.AddRangeAsync(taxes);
                        await _voucherTaxRepo.SaveChanges();

                        _logger.LogInformation("✅ Saved {Count} tax lines", taxes.Count);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error processing tax lines");
                return ResponseViewModel<ExpenseVoucherResponseDto>.Error("Error processing tax lines", FinanceErrorCode.InternalServerError);
            }

            // 8. Enhanced Attachments Processing
            try
            {
                if (dto.Attachments?.Any() == true)
                {
                    var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                    if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

                    foreach (var file in dto.Attachments)
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
                        using (var sha256 = SHA256.Create())
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

                        _logger.LogInformation("✅ Processed attachment: {FileName}, Size: {Size} bytes, MimeType: {MimeType}",
                            file.FileName, file.Length, mimeType);
                    }

                    if (attachments.Any())
                    {
                        await _attachmentRepo.AddRangeAsync(attachments);
                        await _attachmentRepo.SaveChanges();
                        _logger.LogInformation("✅ Saved {Count} attachments to database", attachments.Count);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error processing attachments");
                return ResponseViewModel<ExpenseVoucherResponseDto>.Error("Error processing attachments", FinanceErrorCode.InternalServerError);
            }

            // 9. Response
            try
            {
                var response = _mapper.Map<ExpenseVoucherResponseDto>(voucher);
                response.Source = dto.Source;
                response.Counterparty = dto.Counterparty;

                // Load tax details with profile and component names
                response.TaxLines = await _context.VoucherTaxes
                    .Where(vt => vt.VoucherId == voucher.Id)
                    .Include(vt => vt.TaxProfile)
                    .Include(vt => vt.TaxComponent)
                    .Select(vt => new TaxLineResponseDto
                    {
                        TaxProfileId = vt.TaxProfileId,
                        TaxComponentId = vt.TaxComponentId,
                        TaxProfileName = vt.TaxProfile.Name,
                        TaxComponentName = vt.TaxComponent.Name,
                        BaseAmount = vt.BaseAmount,
                        TaxAmount = vt.TaxAmount,
                        AppliedRate = vt.AppliedRate,
                        IsWithholding = vt.IsWithholding
                    })
                    .ToListAsync(cancellationToken);

                response.Attachments = _mapper.Map<List<AttachmentResponseDto>>(attachments);

                return ResponseViewModel<ExpenseVoucherResponseDto>.Success(response, "Expense voucher created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error creating response");
                return ResponseViewModel<ExpenseVoucherResponseDto>.Error("Error creating response", FinanceErrorCode.InternalServerError);
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