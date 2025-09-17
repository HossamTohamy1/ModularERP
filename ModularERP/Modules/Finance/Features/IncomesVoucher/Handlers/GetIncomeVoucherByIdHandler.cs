using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.IncomesVoucher.DTO;
using ModularERP.Modules.Finance.Features.IncomesVoucher.Queries;
using ModularERP.Modules.Finance.Features.Vouchers.Models;
using ModularERP.Modules.Finance.Features.VoucherTaxs.Models;
using ModularERP.Modules.Finance.Features.Attachments.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Finance.Features.IncomesVoucher.Handlers
{
    public class GetIncomeVoucherByIdHandler : IRequestHandler<GetIncomeVoucherByIdQuery, ResponseViewModel<IncomeVoucherResponseDto>>
    {
        private readonly IGeneralRepository<Voucher> _voucherRepo;
        private readonly IGeneralRepository<VoucherTax> _voucherTaxRepo;
        private readonly IGeneralRepository<VoucherAttachment> _attachmentRepo;
        private readonly IMapper _mapper;

        public GetIncomeVoucherByIdHandler(
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

        public async Task<ResponseViewModel<IncomeVoucherResponseDto>> Handle(GetIncomeVoucherByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var voucher = await _voucherRepo.GetByID(request.Id);

                if (voucher == null || voucher.Type != VoucherType.Income)
                    return ResponseViewModel<IncomeVoucherResponseDto>.Error(
                        "Income voucher not found",
                        FinanceErrorCode.NotFound);

                var taxLines = await _voucherTaxRepo.Get(t => t.VoucherId == voucher.Id).ToListAsync(cancellationToken);
                var attachments = await _attachmentRepo.Get(a => a.VoucherId == voucher.Id).ToListAsync(cancellationToken);

                var response = _mapper.Map<IncomeVoucherResponseDto>(voucher);
                response.TaxLines = _mapper.Map<List<TaxLineResponseDto>>(taxLines);
                response.Attachments = _mapper.Map<List<AttachmentResponseDto>>(attachments);

                return ResponseViewModel<IncomeVoucherResponseDto>.Success(response);
            }
            catch (Exception)
            {
                return ResponseViewModel<IncomeVoucherResponseDto>.Error(
                    "An error occurred while fetching the income voucher",
                    FinanceErrorCode.InternalServerError);
            }
        }
    }
}
