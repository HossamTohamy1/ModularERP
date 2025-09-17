using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.ExpensesVoucher.DTO;
using ModularERP.Modules.Finance.Features.ExpensesVoucher.Queries;
using ModularERP.Modules.Finance.Features.Vouchers.Models;
using ModularERP.Modules.Finance.Features.VoucherTaxs.Models;
using ModularERP.Modules.Finance.Features.Attachments.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Finance.Features.ExpensesVoucher.Handlers
{
    public class GetExpenseVoucherByIdHandler : IRequestHandler<GetExpenseVoucherByIdQuery, ResponseViewModel<ExpenseVoucherResponseDto>>
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

        public async Task<ResponseViewModel<ExpenseVoucherResponseDto>> Handle(GetExpenseVoucherByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var voucher = await _voucherRepo.GetByID(request.Id);

                if (voucher == null || voucher.Type != VoucherType.Expense)
                    return ResponseViewModel<ExpenseVoucherResponseDto>.Error(
                        "Expense voucher not found",
                        FinanceErrorCode.NotFound);

                var taxLines = await _voucherTaxRepo.Get(t => t.VoucherId == voucher.Id).ToListAsync(cancellationToken);
                var attachments = await _attachmentRepo.Get(a => a.VoucherId == voucher.Id).ToListAsync(cancellationToken);

                var response = _mapper.Map<ExpenseVoucherResponseDto>(voucher);
                response.TaxLines = _mapper.Map<List<TaxLineResponseDto>>(taxLines);
                response.Attachments = _mapper.Map<List<AttachmentResponseDto>>(attachments);

                return ResponseViewModel<ExpenseVoucherResponseDto>.Success(response);
            }
            catch (Exception ex)
            {
                // هنا ممكن تضيف logging لو تحب
                return ResponseViewModel<ExpenseVoucherResponseDto>.Error(
                    "An error occurred while fetching the expense voucher",
                    FinanceErrorCode.InternalServerError);
            }
        }
    }
}
