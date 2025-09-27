using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.Attachments.Models;
using ModularERP.Modules.Finance.Features.ExpensesVoucher.Commands;
using ModularERP.Modules.Finance.Features.ExpensesVoucher.DTO;
using ModularERP.Modules.Finance.Features.ExpensesVoucher.Service;
using ModularERP.Modules.Finance.Features.Vouchers.Models;
using ModularERP.Modules.Finance.Features.VoucherTaxs.Models;
using ModularERP.Shared.Interfaces;
using System.Text.Json;

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

        public CreateExpenseVoucherHandler(
            IGeneralRepository<Voucher> voucherRepo,
            IGeneralRepository<VoucherTax> voucherTaxRepo,
            IGeneralRepository<VoucherAttachment> attachmentRepo,
            IExpenseVoucherService expenseService,
            IMapper mapper,
            IValidator<CreateExpenseVoucherDto> validator,
            ILogger<CreateExpenseVoucherHandler> logger)
        {
            _voucherRepo = voucherRepo;
            _voucherTaxRepo = voucherTaxRepo;
            _attachmentRepo = attachmentRepo;
            _expenseService = expenseService;
            _mapper = mapper;
            _validator = validator;
            _logger = logger;
        }

        public async Task<ResponseViewModel<ExpenseVoucherResponseDto>> Handle(CreateExpenseVoucherCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var dto = request.Request;

                _logger.LogInformation("Start creating expense voucher with data: {@Dto}", new
                {
                    dto.Code,
                    dto.Date,
                    dto.Amount,
                    dto.Description,
                    dto.CategoryId,
                    Source = dto.Source,
                    Counterparty = dto.Counterparty,
                    TaxLinesCount = dto.TaxLines?.Count ?? 0,
                    AttachmentsCount = dto.Attachments?.Count ?? 0
                });

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

                _logger.LogInformation("Validation passed successfully.");

                // Business validations using Source
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

                _logger.LogInformation("Business validations passed successfully.");

                // Generate or use provided code
                var code = string.IsNullOrEmpty(dto.Code)
                    ? await _expenseService.GenerateVoucherCodeAsync(VoucherType.Expense, dto.Date)
                    : dto.Code;

                // Check duplicate before insert
                var exists = await _voucherRepo.AnyAsync(v =>
                    v.CompanyId == Guid.Parse("1edafb1b-de49-4fc8-b06a-d563864b9227") &&
                    v.Code == code);

                if (exists)
                {
                    return ResponseViewModel<ExpenseVoucherResponseDto>.Error(
                        $"Voucher code '{code}' already exists for this company.",
                        FinanceErrorCode.BusinessLogicError);
                }

                // Create voucher
                var voucher = _mapper.Map<Voucher>(dto);
                voucher.Code = code;
                voucher.CompanyId = Guid.Parse("1edafb1b-de49-4fc8-b06a-d563864b9227");
                voucher.CreatedBy = Guid.Parse("f0602c31-0c12-4b5c-9ccf-fe17811d5c53");

                // Use Source for JournalAccountId
                voucher.JournalAccountId = await _expenseService.GetWalletControlAccountIdAsync(dto.Source.Id, dto.Source.Type);

                _logger.LogInformation("Adding voucher to repository: {@Voucher}", new { voucher.Code, voucher.Amount, voucher.Date });

                await _voucherRepo.AddAsync(voucher);
                await _voucherRepo.SaveChanges();

                _logger.LogInformation("Voucher saved successfully with ID: {VoucherId}", voucher.Id);

                // ✅ Process Tax Lines (same as Income)
                var taxes = new List<VoucherTax>();
                if (dto.TaxLines?.Any() == true)
                {
                    _logger.LogInformation("Processing {Count} tax lines", dto.TaxLines.Count);

                    foreach (var taxLineDto in dto.TaxLines)
                    {
                        _logger.LogInformation("Processing tax line: TaxId={TaxId}, BaseAmount={BaseAmount}, TaxAmount={TaxAmount}",
                            taxLineDto.TaxId, taxLineDto.BaseAmount, taxLineDto.TaxAmount);

                        taxes.Add(new VoucherTax
                        {
                            VoucherId = voucher.Id,
                            TaxId = taxLineDto.TaxId,
                            BaseAmount = taxLineDto.BaseAmount,
                            TaxAmount = taxLineDto.TaxAmount,
                            IsWithholding = taxLineDto.IsWithholding,
                            Direction = TaxDirection.Expense // Use TaxDirection enum
                        });
                    }

                    await _voucherTaxRepo.AddRangeAsync(taxes);
                    await _voucherTaxRepo.SaveChanges();

                    _logger.LogInformation("Successfully added {Count} tax lines", taxes.Count);
                }
                else
                {
                    _logger.LogInformation("No tax lines provided");
                }

                // Handle attachments with enhanced properties
                var attachments = new List<VoucherAttachment>();
                if (dto.Attachments?.Any() == true)
                {
                    _logger.LogInformation("Processing {Count} attachments", dto.Attachments.Count);

                    var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                    if (!Directory.Exists(uploadDir))
                        Directory.CreateDirectory(uploadDir);

                    foreach (var file in dto.Attachments)
                    {
                        // Validate file
                        if (file.Length == 0)
                        {
                            _logger.LogWarning("Skipping empty file: {FileName}", file.FileName);
                            continue;
                        }

                        // Generate unique filename
                        var fileName = $"{Guid.NewGuid()}_{file.FileName}";
                        var filePath = Path.Combine(uploadDir, fileName);
                        string checksum = null;

                        // Save file to disk first
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(fileStream, cancellationToken);
                        }

                        // Calculate checksum from saved file
                        using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                        using (var sha256 = System.Security.Cryptography.SHA256.Create())
                        {
                            var hashBytes = await sha256.ComputeHashAsync(fileStream, cancellationToken);
                            checksum = Convert.ToHexString(hashBytes);
                        }

                        // Get MIME type
                        var mimeType = GetMimeType(file.FileName, file.ContentType);

                        var attachment = new VoucherAttachment
                        {
                            VoucherId = voucher.Id,
                            FilePath = $"/uploads/{fileName}",
                            Filename = file.FileName,
                            MimeType = mimeType,
                            FileSize = (int)file.Length,
                            Checksum = checksum,
                            UploadedBy = Guid.Parse("f0602c31-0c12-4b5c-9ccf-fe17811d5c53"),
                            UploadedAt = DateTime.UtcNow
                        };

                        // Add to repository immediately
                        await _attachmentRepo.AddAsync(attachment);
                        attachments.Add(attachment);

                        _logger.LogInformation("✅ Processed attachment: {FileName}, Size: {Size} bytes, MimeType: {MimeType}",
                            file.FileName, file.Length, mimeType);
                    }

                    // Save all attachments
                    await _attachmentRepo.SaveChanges();
                    _logger.LogInformation("Successfully added {Count} attachments", attachments.Count);
                }

                // Enhanced Response mapping with database fetch
                _logger.LogInformation("Voucher {VoucherId} created successfully with {TaxCount} taxes and {AttachmentCount} attachments.",
                    voucher.Id, taxes.Count, attachments.Count);

                // Fetch saved data from database
                var savedTaxes = await _voucherTaxRepo.Get(t => t.VoucherId == voucher.Id).ToListAsync(cancellationToken);
                var savedAttachments = await _attachmentRepo.Get(a => a.VoucherId == voucher.Id).ToListAsync(cancellationToken);

                var response = _mapper.Map<ExpenseVoucherResponseDto>(voucher);

                // Set Source and Counterparty from DTO
                response.Source = dto.Source;
                response.Counterparty = dto.Counterparty;

                // Map collections using AutoMapper
                response.TaxLines = _mapper.Map<List<TaxLineResponseDto>>(savedTaxes);
                response.Attachments = _mapper.Map<List<AttachmentResponseDto>>(savedAttachments);

                _logger.LogInformation("Response created with {AttachmentCount} attachments properly mapped from database",
                    response.Attachments.Count);

                return ResponseViewModel<ExpenseVoucherResponseDto>.Success(response, "Expense voucher created successfully");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error occurred while creating expense voucher.");
                return ResponseViewModel<ExpenseVoucherResponseDto>.Error(
                    "An error occurred while creating the expense voucher", FinanceErrorCode.InternalServerError);
            }
        }
        /// <summary>
        /// Get MIME type from file extension and content type
        /// </summary>
        private string GetMimeType(string fileName, string contentType)
        {
            // Use provided content type if valid and not generic
            if (!string.IsNullOrEmpty(contentType) &&
                contentType != "application/octet-stream" &&
                !contentType.Contains("form-data"))
            {
                return contentType;
            }

            // Fallback to extension-based detection
            var extension = Path.GetExtension(fileName)?.ToLowerInvariant();

            return extension switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".ppt" => "application/vnd.ms-powerpoint",
                ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".webp" => "image/webp",
                ".svg" => "image/svg+xml",
                ".txt" => "text/plain",
                ".csv" => "text/csv",
                ".json" => "application/json",
                ".xml" => "application/xml",
                ".zip" => "application/zip",
                ".rar" => "application/x-rar-compressed",
                ".7z" => "application/x-7z-compressed",
                ".tar" => "application/x-tar",
                ".gz" => "application/gzip",
                ".mp3" => "audio/mpeg",
                ".wav" => "audio/wav",
                ".mp4" => "video/mp4",
                ".avi" => "video/x-msvideo",
                ".mov" => "video/quicktime",
                _ => "application/octet-stream"
            };
        }


    }
}