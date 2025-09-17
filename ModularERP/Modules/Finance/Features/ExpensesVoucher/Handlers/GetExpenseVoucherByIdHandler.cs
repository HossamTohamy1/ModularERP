using AutoMapper;
using MediatR;
using ModularERP.Modules.Finance.Features.ExpensesVoucher.DTO;
using ModularERP.Modules.Finance.Features.ExpensesVoucher.Queries;
using ModularERP.Modules.Finance.Features.Vouchers.Models;
using ModularERP.Modules.Finance.Features.VoucherTaxs.Models;
using ModularERP.Modules.Finance.Features.Attachments.Models;
using ModularERP.Shared.Interfaces;
using ModularERP.Common.Enum.Finance_Enum;
using Microsoft.EntityFrameworkCore;

namespace ModularERP.Modules.Finance.Features.ExpensesVoucher.Handlers
{
    public class GetExpenseVoucherByIdHandler : IRequestHandler<GetExpenseVoucherByIdQuery, ExpenseVoucherResponseDto?>
    {
        private readonly IGeneralRepository<Voucher> _voucherRepo;
        private readonly IGeneralRepository<VoucherTax> _voucherTaxRepo;
        private readonly IGeneralRepository<VoucherAttachment> _attachmentRepo;
        private readonly IMapper _mapper;

        public GetExpenseVoucherByIdHandler(
            IGeneralRepository<Voucher> voucherRepo,
            IGeneralRepository<VoucherTax> voucherTaxRepo,
            IGeneralRepository<VoucherAttachment> attachmentRepo,
            IMapper mapper)
        {
            _voucherRepo = voucherRepo;
            _voucherTaxRepo = voucherTaxRepo;
            _attachmentRepo = attachmentRepo;
            _mapper = mapper;
        }

        public async Task<ExpenseVoucherResponseDto?> Handle(GetExpenseVoucherByIdQuery request, CancellationToken cancellationToken)
        {
            var voucher = await _voucherRepo.GetByID(request.Id);

            if (voucher == null || voucher.Type != VoucherType.Expense)
                return null;

            var taxLines = await _voucherTaxRepo.Get(t => t.VoucherId == voucher.Id).ToListAsync();

            var attachments = await _attachmentRepo.Get(a => a.VoucherId == voucher.Id).ToListAsync();

            var response = _mapper.Map<ExpenseVoucherResponseDto>(voucher);
            response.TaxLines = _mapper.Map<List<TaxLineResponseDto>>(taxLines);
            response.Attachments = _mapper.Map<List<AttachmentResponseDto>>(attachments);

            return response;
        }
    }
}
