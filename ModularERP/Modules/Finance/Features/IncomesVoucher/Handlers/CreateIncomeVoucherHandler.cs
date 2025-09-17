using AutoMapper;
using FluentValidation;
using MediatR;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.Attachments.Models;
using ModularERP.Modules.Finance.Features.IncomesVoucher.Commands;
using ModularERP.Modules.Finance.Features.IncomesVoucher.DTO;
using ModularERP.Modules.Finance.Features.IncomesVoucher.Service.Interface;
using ModularERP.Modules.Finance.Features.Vouchers.Models;
using ModularERP.Modules.Finance.Features.VoucherTaxs.Models;
using ModularERP.Shared.Interfaces;

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

        public CreateIncomeVoucherHandler(
            IGeneralRepository<Voucher> voucherRepo,
            IGeneralRepository<VoucherTax> voucherTaxRepo,
            IGeneralRepository<VoucherAttachment> attachmentRepo,
            IIncomeVoucherService incomeService,
            IMapper mapper,
            IValidator<CreateIncomeVoucherDto> validator)
        {
            _voucherRepo = voucherRepo;
            _voucherTaxRepo = voucherTaxRepo;
            _attachmentRepo = attachmentRepo;
            _incomeService = incomeService;
            _mapper = mapper;
            _validator = validator;
        }

        public async Task<ResponseViewModel<IncomeVoucherResponseDto>> Handle(CreateIncomeVoucherCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var dto = request.Request;

                // Validate DTO
                var validationResult = await _validator.ValidateAsync(dto, cancellationToken);
                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

                    return ResponseViewModel<IncomeVoucherResponseDto>.ValidationError(
                        "Validation failed", errors);
                }

                // Business validations
                if (!await _incomeService.IsWalletActiveAsync(dto.Source.Id, dto.Source.Type))
                    return ResponseViewModel<IncomeVoucherResponseDto>.Error(
                        "Selected wallet is not active", FinanceErrorCode.BusinessLogicError);

                if (!await _incomeService.IsCategoryValidAsync(dto.CategoryId))
                    return ResponseViewModel<IncomeVoucherResponseDto>.Error(
                        "Invalid revenue category", FinanceErrorCode.BusinessLogicError);

                if (!await _incomeService.IsDateInOpenPeriodAsync(dto.Date))
                    return ResponseViewModel<IncomeVoucherResponseDto>.Error(
                        "Date is not within open fiscal period", FinanceErrorCode.BusinessLogicError);

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

                // Create voucher entity
                var voucher = _mapper.Map<Voucher>(dto);
                voucher.Code = code;
                voucher.CompanyId = Guid.Parse("d00cf078-c1ff-4feb-ae37-d66a827ae438"); // temporary until auth works
                voucher.CreatedBy = Guid.Parse("5dbceb39-9677-46d7-bf8a-2eb914c55437"); // temporary until auth works
                voucher.JournalAccountId = await _incomeService.GetWalletControlAccountIdAsync(dto.Source.Id, dto.Source.Type);

                await _voucherRepo.AddAsync(voucher);
                await _voucherRepo.SaveChanges();

                // Create tax lines
                var taxes = _mapper.Map<List<VoucherTax>>(dto.TaxLines);
                taxes.ForEach(t => t.VoucherId = voucher.Id);
                foreach (var tax in taxes)
                    await _voucherTaxRepo.AddAsync(tax);

                // Create attachments
                var attachments = dto.Attachments.Select(url => new VoucherAttachment
                {
                    VoucherId = voucher.Id,
                    FilePath = url,
                    Filename = Path.GetFileName(url),
                    UploadedBy = Guid.Parse("5dbceb39-9677-46d7-bf8a-2eb914c55437"), // temporary until auth works
                    UploadedAt = DateTime.UtcNow
                }).ToList();

                foreach (var attachment in attachments)
                    await _attachmentRepo.AddAsync(attachment);

                await _voucherRepo.SaveChanges();

                // Map response
                var response = _mapper.Map<IncomeVoucherResponseDto>(voucher);
                response.TaxLines = _mapper.Map<List<TaxLineResponseDto>>(taxes);
                response.Attachments = _mapper.Map<List<AttachmentResponseDto>>(attachments);

                return ResponseViewModel<IncomeVoucherResponseDto>.Success(response, "Income voucher created successfully");
            }
            catch (Exception)
            {
                return ResponseViewModel<IncomeVoucherResponseDto>.Error(
                    "An error occurred while creating the income voucher", FinanceErrorCode.InternalServerError);
            }
        }
    }
}