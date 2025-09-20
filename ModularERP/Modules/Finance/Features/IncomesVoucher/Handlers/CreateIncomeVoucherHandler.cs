using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.Attachments.Models;
using ModularERP.Modules.Finance.Features.IncomesVoucher.Commands;
using ModularERP.Modules.Finance.Features.IncomesVoucher.DTO;
using ModularERP.Modules.Finance.Features.IncomesVoucher.Service.Interface;
using ModularERP.Modules.Finance.Features.Vouchers.Models;
using ModularERP.Modules.Finance.Features.VoucherTaxs.Models;
using ModularERP.Shared.Interfaces;
using System.Text.Json;

namespace ModularERP.Modules.Finance.Features.IncomesVoucher.Handlers
{
    public class CreateIncomeVoucherHandler : IRequestHandler<CreateIncomeVoucherCommand, ResponseViewModel<IncomeVoucherResponseDto>>
    {
        private readonly IGeneralRepository<Voucher> _voucherRepo;
        private readonly IGeneralRepository<VoucherTax> _voucherTaxRepo;
        private readonly IGeneralRepository<VoucherAttachment> _attachmentRepo;
        private readonly IIncomeVoucherService _incomeService;
        private readonly IMapper _mapper;
        private readonly IValidator<CreateIncomeVoucherDto> _validator;
        private readonly ILogger<CreateIncomeVoucherHandler> _logger;

        public CreateIncomeVoucherHandler(
            IGeneralRepository<Voucher> voucherRepo,
            IGeneralRepository<VoucherTax> voucherTaxRepo,
            IGeneralRepository<VoucherAttachment> attachmentRepo,
            IIncomeVoucherService incomeService,
            IMapper mapper,
            IValidator<CreateIncomeVoucherDto> validator,
            ILogger<CreateIncomeVoucherHandler> logger)
        {
            _voucherRepo = voucherRepo;
            _voucherTaxRepo = voucherTaxRepo;
            _attachmentRepo = attachmentRepo;
            _incomeService = incomeService;
            _mapper = mapper;
            _validator = validator;
            _logger = logger;
        }

        public async Task<ResponseViewModel<IncomeVoucherResponseDto>> Handle(CreateIncomeVoucherCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var dto = request.Request;

                _logger.LogInformation("Start creating income voucher with data: {@Dto}", new
                {
                    dto.Code,
                    dto.Date,
                    dto.Amount,
                    dto.Description,
                    dto.CategoryId,
                    dto.SourceId,
                    dto.SourceType,
                    SourceJson = dto.SourceJson,
                    TaxLinesJson = dto.TaxLinesJson,
                    CounterpartyJson = dto.CounterpartyJson,
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
                    return ResponseViewModel<IncomeVoucherResponseDto>.ValidationError("Validation failed", errors);
                }
                _logger.LogInformation("Validation passed successfully.");

                // Business validations using SourceId and SourceType
                if (dto.SourceId.HasValue && !string.IsNullOrEmpty(dto.SourceType))
                {
                    if (!await _incomeService.IsWalletActiveAsync(dto.SourceId.Value, dto.SourceType))
                    {
                        return ResponseViewModel<IncomeVoucherResponseDto>.Error(
                            "Selected wallet is not active", FinanceErrorCode.BusinessLogicError);
                    }
                }

                if (!await _incomeService.IsCategoryValidAsync(dto.CategoryId))
                {
                    return ResponseViewModel<IncomeVoucherResponseDto>.Error(
                        "Invalid revenue category", FinanceErrorCode.BusinessLogicError);
                }

                if (!await _incomeService.IsDateInOpenPeriodAsync(dto.Date))
                {
                    return ResponseViewModel<IncomeVoucherResponseDto>.Error(
                        "Date is not within open fiscal period", FinanceErrorCode.BusinessLogicError);
                }

                _logger.LogInformation("Business validations passed successfully.");

                // Generate or use provided code
                var code = string.IsNullOrEmpty(dto.Code)
                    ? await _incomeService.GenerateVoucherCodeAsync(VoucherType.Income, dto.Date)
                    : dto.Code;

                // Check duplicate before insert
                var exists = await _voucherRepo.AnyAsync(v =>
                    v.CompanyId == Guid.Parse("d00cf078-c1ff-4feb-ae37-d66a827ae438") &&
                    v.Code == code);

                if (exists)
                {
                    return ResponseViewModel<IncomeVoucherResponseDto>.Error(
                        $"Voucher code '{code}' already exists for this company.",
                        FinanceErrorCode.BusinessLogicError);
                }

                // Create voucher
                var voucher = _mapper.Map<Voucher>(dto);
                voucher.Code = code;
                voucher.CompanyId = Guid.Parse("d00cf078-c1ff-4feb-ae37-d66a827ae438");
                voucher.CreatedBy = Guid.Parse("5dbceb39-9677-46d7-bf8a-2eb914c55437");

                // Use SourceId and SourceType for JournalAccountId
                if (dto.SourceId.HasValue && !string.IsNullOrEmpty(dto.SourceType))
                {
                    voucher.JournalAccountId = await _incomeService.GetWalletControlAccountIdAsync(dto.SourceId.Value, dto.SourceType);
                }

                _logger.LogInformation("Adding voucher to repository: {@Voucher}", new { voucher.Code, voucher.Amount, voucher.Date });

                await _voucherRepo.AddAsync(voucher);
                await _voucherRepo.SaveChanges();

                _logger.LogInformation("Voucher saved successfully with ID: {VoucherId}", voucher.Id);

                // ✅ Process Tax Lines from JSON
                var taxes = new List<VoucherTax>();
                if (!string.IsNullOrEmpty(dto.TaxLinesJson))
                {
                    try
                    {
                        var taxLinesFromJson = JsonSerializer.Deserialize<List<TaxLineDto>>(dto.TaxLinesJson);
                        if (taxLinesFromJson?.Any() == true)
                        {
                            _logger.LogInformation("Processing {Count} tax lines from JSON", taxLinesFromJson.Count);

                            foreach (var taxLineDto in taxLinesFromJson)
                            {
                                _logger.LogInformation("Processing tax line: TaxId={TaxId}, BaseAmount={BaseAmount}, TaxAmount={TaxAmount}",
                                    taxLineDto.TaxId, taxLineDto.BaseAmount, taxLineDto.TaxAmount);

                                taxes.Add(new VoucherTax
                                {
                                    VoucherId = voucher.Id,
                                    TaxId = taxLineDto.TaxId,
                                    BaseAmount = taxLineDto.BaseAmount,
                                    TaxAmount = taxLineDto.TaxAmount,
                                    IsWithholding = taxLineDto.IsWithholding
                                });
                            }

                            await _voucherTaxRepo.AddRangeAsync(taxes);
                            await _voucherTaxRepo.SaveChanges();

                            _logger.LogInformation("Successfully added {Count} tax lines", taxes.Count);
                        }
                        else
                        {
                            _logger.LogInformation("TaxLinesJson is empty or parsing returned no items");
                        }
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogError(ex, "Error parsing TaxLinesJson: {TaxLinesJson}", dto.TaxLinesJson);
                        // Continue without tax lines - don't fail the whole operation
                    }
                }
                else
                {
                    _logger.LogInformation("No TaxLinesJson provided");
                }

                // Handle attachments
                var attachments = new List<VoucherAttachment>();
                if (dto.Attachments?.Any() == true)
                {
                    _logger.LogInformation("Processing {Count} attachments", dto.Attachments.Count);

                    var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                    if (!Directory.Exists(uploadDir))
                        Directory.CreateDirectory(uploadDir);

                    foreach (var file in dto.Attachments)
                    {
                        var fileName = $"{Guid.NewGuid()}_{file.FileName}";
                        var filePath = Path.Combine(uploadDir, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream, cancellationToken);
                        }

                        attachments.Add(new VoucherAttachment
                        {
                            VoucherId = voucher.Id,
                            FilePath = $"/uploads/{fileName}",
                            Filename = file.FileName,
                            UploadedBy = Guid.Parse("5dbceb39-9677-46d7-bf8a-2eb914c55437"),
                            UploadedAt = DateTime.UtcNow
                        });
                    }

                    await _attachmentRepo.AddRangeAsync(attachments);
                    await _attachmentRepo.SaveChanges();

                    _logger.LogInformation("Successfully added {Count} attachments", attachments.Count);
                }

                // في نهاية Handler - Response mapping المحسن
                _logger.LogInformation("Voucher {VoucherId} created successfully with {TaxCount} taxes and {AttachmentCount} attachments.",
                    voucher.Id, taxes.Count, attachments.Count);

             
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

                // Map attachments from database
                response.Attachments = attachments.Select(a => new AttachmentResponseDto
                {
                    Id = a.Id,
                    FileName = a.Filename,
                    FileUrl = a.FilePath
                }).ToList();

                return ResponseViewModel<IncomeVoucherResponseDto>.Success(response, "Income voucher created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error occurred while creating income voucher.");
                return ResponseViewModel<IncomeVoucherResponseDto>.Error(
                    "An error occurred while creating the income voucher", FinanceErrorCode.InternalServerError);
            }
        }
    }
}