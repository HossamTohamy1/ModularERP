using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.ExpensesVoucher.Commands;
using ModularERP.Modules.Finance.Features.ExpensesVoucher.DTO;
using ModularERP.Modules.Finance.Features.ExpensesVoucher.Service;
using ModularERP.Modules.Finance.Features.Vouchers.Models;
using ModularERP.Modules.Finance.Features.VoucherTaxs.Models;
using ModularERP.Modules.Finance.Features.Attachments.Models;
using ModularERP.Shared.Interfaces;

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

        public CreateExpenseVoucherHandler(
            IGeneralRepository<Voucher> voucherRepo,
            IGeneralRepository<VoucherTax> voucherTaxRepo,
            IGeneralRepository<VoucherAttachment> attachmentRepo,
            IExpenseVoucherService expenseService,
            IMapper mapper,
            IValidator<CreateExpenseVoucherDto> validator)
        {
            _voucherRepo = voucherRepo;
            _voucherTaxRepo = voucherTaxRepo;
            _attachmentRepo = attachmentRepo;
            _expenseService = expenseService;
            _mapper = mapper;
            _validator = validator;
        }

        public async Task<ResponseViewModel<ExpenseVoucherResponseDto>> Handle(CreateExpenseVoucherCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var dto = request.Request;

                // Validate
                var validationResult = await _validator.ValidateAsync(dto, cancellationToken);
                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

                    return ResponseViewModel<ExpenseVoucherResponseDto>.ValidationError(
                        "Validation failed", errors);
                }

                // Business validations
                if (!await _expenseService.IsWalletActiveAsync(dto.Source.Id, dto.Source.Type))
                    return ResponseViewModel<ExpenseVoucherResponseDto>.Error(
                        "Selected wallet is not active",
                        FinanceErrorCode.BusinessLogicError);

                if (!await _expenseService.IsCategoryValidAsync(dto.CategoryId))
                    return ResponseViewModel<ExpenseVoucherResponseDto>.Error(
                        "Invalid expense category",
                        FinanceErrorCode.BusinessLogicError);

                if (!await _expenseService.IsDateInOpenPeriodAsync(dto.Date))
                    return ResponseViewModel<ExpenseVoucherResponseDto>.Error(
                        "Date is not within open fiscal period",
                        FinanceErrorCode.BusinessLogicError);

                // Generate code
                var code = string.IsNullOrEmpty(dto.Code)
                    ? await _expenseService.GenerateVoucherCodeAsync(VoucherType.Expense, dto.Date)
                    : dto.Code;

                var voucher = _mapper.Map<Voucher>(dto);
                voucher.Code = code;
                voucher.CompanyId = Guid.Parse("d00cf078-c1ff-4feb-ae37-d66a827ae438");
                voucher.CreatedBy = Guid.Parse("5dbceb39-9677-46d7-bf8a-2eb914c55437"); // temporary because request.UserId not working (Auth);
                voucher.JournalAccountId = await _expenseService.GetWalletControlAccountIdAsync(dto.Source.Id, dto.Source.Type);

                // Save voucher
                await _voucherRepo.AddAsync(voucher);
                await _voucherRepo.SaveChanges();

                // Tax lines
                foreach (var taxLine in dto.TaxLines)
                {
                    var voucherTax = _mapper.Map<VoucherTax>(taxLine);
                    voucherTax.VoucherId = voucher.Id;
                    await _voucherTaxRepo.AddAsync(voucherTax);
                }

                // Attachments
                foreach (var attachmentUrl in dto.Attachments)
                {
                    var attachment = new VoucherAttachment
                    {
                        VoucherId = voucher.Id,
                        FilePath = attachmentUrl,
                        Filename = Path.GetFileName(attachmentUrl),
                        UploadedBy = Guid.Parse("5dbceb39-9677-46d7-bf8a-2eb914c55437"), // temporary because request.UserId not working (Auth),
                        UploadedAt = DateTime.UtcNow
                    };
                    await _attachmentRepo.AddAsync(attachment);
                }

                await _voucherRepo.SaveChanges();

                // Map response
                var taxLines = await _voucherTaxRepo.Get(t => t.VoucherId == voucher.Id).ToListAsync();
                var attachments = await _attachmentRepo.Get(a => a.VoucherId == voucher.Id).ToListAsync();

                var response = _mapper.Map<ExpenseVoucherResponseDto>(voucher);
                response.TaxLines = _mapper.Map<List<TaxLineResponseDto>>(taxLines);
                response.Attachments = _mapper.Map<List<AttachmentResponseDto>>(attachments);

                return ResponseViewModel<ExpenseVoucherResponseDto>.Success(response, "Expense voucher created successfully");
            }
            catch (Exception ex)
            {
                return ResponseViewModel<ExpenseVoucherResponseDto>.Error(
                    "An error occurred while creating the expense voucher",
                    FinanceErrorCode.InternalServerError);
            }
        }
    }
}
