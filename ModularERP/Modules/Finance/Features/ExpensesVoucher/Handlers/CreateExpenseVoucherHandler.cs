using AutoMapper;
using FluentValidation;
using MediatR;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Modules.Finance.Features.Attachments.Models;
using ModularERP.Modules.Finance.Features.ExpensesVoucher.Commands;
using ModularERP.Modules.Finance.Features.ExpensesVoucher.DTO;
using ModularERP.Modules.Finance.Features.ExpensesVoucher.Service;
using ModularERP.Modules.Finance.Features.Vouchers.Models;
using ModularERP.Modules.Finance.Features.VoucherTaxs.Models;
using ModularERP.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ModularERP.Modules.Finance.Features.ExpensesVoucher.Handlers
{
    public class CreateExpenseVoucherHandler : IRequestHandler<CreateExpenseVoucherCommand, ExpenseVoucherResponseDto>
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

        public async Task<ExpenseVoucherResponseDto> Handle(CreateExpenseVoucherCommand request, CancellationToken cancellationToken)
        {
            var dto = request.Request;

            //  Validate request
            var validationResult = await _validator.ValidateAsync(dto, cancellationToken);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            // Business validations
            if (!await _expenseService.IsWalletActiveAsync(dto.Source.Id, dto.Source.Type))
                throw new InvalidOperationException("Selected wallet is not active");

            if (!await _expenseService.IsCategoryValidAsync(dto.CategoryId))
                throw new InvalidOperationException("Invalid expense category");

            if (!await _expenseService.IsDateInOpenPeriodAsync(dto.Date))
                throw new InvalidOperationException("Date is not within open fiscal period");

            // 3️⃣ Generate code if not provided
            var code = string.IsNullOrEmpty(dto.Code)
                ? await _expenseService.GenerateVoucherCodeAsync(VoucherType.Expense, dto.Date)
                : dto.Code;

            //  Create voucher entity
            var voucher = _mapper.Map<Voucher>(dto);
            voucher.Code = code;
            voucher.CompanyId = Guid.Parse("d00cf078-c1ff-4feb-ae37-d66a827ae438");
            voucher.CreatedBy = Guid.Parse("5dbceb39-9677-46d7-bf8a-2eb914c55437"); // temporary because request.UserId not working (Auth)

            //  Assign JournalAccountId based on wallet
            voucher.JournalAccountId = await _expenseService.GetWalletControlAccountIdAsync(dto.Source.Id, dto.Source.Type);

            // 6️⃣ Save voucher
            await _voucherRepo.AddAsync(voucher);
            await _voucherRepo.SaveChanges();

            //  Create tax lines
            foreach (var taxLine in dto.TaxLines)
            {
                var voucherTax = _mapper.Map<VoucherTax>(taxLine);
                voucherTax.VoucherId = voucher.Id;
                await _voucherTaxRepo.AddAsync(voucherTax);
            }

            //  Create attachments
            foreach (var attachmentUrl in dto.Attachments)
            {
                var attachment = new VoucherAttachment
                {
                    VoucherId = voucher.Id,
                    FilePath = attachmentUrl,
                    Filename = Path.GetFileName(attachmentUrl),
                    UploadedBy = Guid.Parse("5dbceb39-9677-46d7-bf8a-2eb914c55437"), // temporary because request.UserId not working (Auth)
                    UploadedAt = DateTime.UtcNow
                };
                await _attachmentRepo.AddAsync(attachment);
            }

            await _voucherRepo.SaveChanges();

            // Load tax lines and attachments manually
            var taxLines = await _voucherTaxRepo.Get(t => t.VoucherId == voucher.Id).ToListAsync();
            var attachments = await _attachmentRepo.Get(a => a.VoucherId == voucher.Id).ToListAsync();

            //  Map to response DTO
            var response = _mapper.Map<ExpenseVoucherResponseDto>(voucher);
            response.TaxLines = _mapper.Map<List<TaxLineResponseDto>>(taxLines);
            response.Attachments = _mapper.Map<List<AttachmentResponseDto>>(attachments);

            return response;
        }
    }
}
