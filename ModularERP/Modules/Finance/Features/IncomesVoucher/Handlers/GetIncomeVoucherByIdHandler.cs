using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.IncomesVoucher.DTO;
using ModularERP.Modules.Finance.Features.IncomesVoucher.Queries;
using ModularERP.Modules.Finance.Features.Vouchers.Models;
using ModularERP.Modules.Finance.Features.VoucherTaxs.Models;
using ModularERP.Modules.Finance.Features.Attachments.Models;
using ModularERP.Shared.Interfaces;
using ModularERP.Modules.Finance.Features.IncomesVoucher.DTO.ModularERP.Modules.Finance.Features.IncomesVoucher.DTO;

namespace ModularERP.Modules.Finance.Features.IncomesVoucher.Handlers
{
    public class GetIncomeVoucherByIdHandler : IRequestHandler<GetIncomeVoucherByIdQuery, ResponseViewModel<IncomeVoucherResponseDto>>
    {
        private readonly IGeneralRepository<Voucher> _voucherRepo;
        private readonly IGeneralRepository<VoucherTax> _voucherTaxRepo;
        private readonly IGeneralRepository<VoucherAttachment> _attachmentRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<GetIncomeVoucherByIdHandler> _logger;

        public GetIncomeVoucherByIdHandler(
            IGeneralRepository<Voucher> voucherRepo,
            IGeneralRepository<VoucherTax> voucherTaxRepo,
            IGeneralRepository<VoucherAttachment> attachmentRepo,
            IMapper mapper,
            ILogger<GetIncomeVoucherByIdHandler> logger)
        {
            _voucherRepo = voucherRepo;
            _voucherTaxRepo = voucherTaxRepo;
            _attachmentRepo = attachmentRepo;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<IncomeVoucherResponseDto>> Handle(GetIncomeVoucherByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Fetching income voucher with ID: {VoucherId}", request.Id);

                // 1. Get voucher with validation
                var voucher = await _voucherRepo.GetByID(request.Id);
                if (voucher == null)
                {
                    _logger.LogWarning("Voucher with ID {VoucherId} not found", request.Id);
                    return ResponseViewModel<IncomeVoucherResponseDto>.Error(
                        "Voucher not found",
                        FinanceErrorCode.NotFound);
                }

                if (voucher.Type != VoucherType.Income)
                {
                    _logger.LogWarning("Voucher with ID {VoucherId} is not an income voucher. Type: {VoucherType}",
                        request.Id, voucher.Type);
                    return ResponseViewModel<IncomeVoucherResponseDto>.Error(
                        "Requested voucher is not an income voucher",
                        FinanceErrorCode.NotFound);
                }

                _logger.LogInformation("Found income voucher: {VoucherCode}", voucher.Code);

                // 2. Get related data sequentially to avoid DbContext threading issues
                var taxLines = await _voucherTaxRepo
                    .Get(t => t.VoucherId == voucher.Id)
                    .Include(t => t.TaxProfile)
                    .Include(t => t.TaxComponent)
                    .ToListAsync(cancellationToken);
                var attachments = await _attachmentRepo
                    .Get(a => a.VoucherId == voucher.Id)
                    .ToListAsync(cancellationToken);

                _logger.LogInformation("Retrieved {TaxCount} tax lines and {AttachmentCount} attachments for voucher {VoucherId}",
                    taxLines.Count, attachments.Count, request.Id);

                // 3. Map to response DTO - AutoMapper will handle Source & Counterparty via custom resolvers
                var response = _mapper.Map<IncomeVoucherResponseDto>(voucher);

                // Map related collections
                response.TaxLines = _mapper.Map<List<TaxLineResponseDto>>(taxLines);
                response.Attachments = _mapper.Map<List<AttachmentResponseDto>>(attachments);

                _logger.LogInformation("Successfully retrieved income voucher {VoucherCode} with all related data",
                    voucher.Code);

                return ResponseViewModel<IncomeVoucherResponseDto>.Success(response,
                    "Income voucher retrieved successfully");
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Request was cancelled while fetching income voucher {VoucherId}", request.Id);
                return ResponseViewModel<IncomeVoucherResponseDto>.Error(
                    "Request was cancelled",
                    FinanceErrorCode.RequestCancelled);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error occurred while fetching income voucher {VoucherId}", request.Id);
                return ResponseViewModel<IncomeVoucherResponseDto>.Error(
                    "An error occurred while fetching the income voucher",
                    FinanceErrorCode.InternalServerError);
            }
        }
    }
}