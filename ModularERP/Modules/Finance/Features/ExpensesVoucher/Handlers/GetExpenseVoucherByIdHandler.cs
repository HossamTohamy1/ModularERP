using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<GetExpenseVoucherByIdHandler> _logger;

        public GetExpenseVoucherByIdHandler(
            IGeneralRepository<Voucher> voucherRepo,
            IGeneralRepository<VoucherTax> voucherTaxRepo,
            IGeneralRepository<VoucherAttachment> attachmentRepo,
            IMapper mapper,
            ILogger<GetExpenseVoucherByIdHandler> logger)
        {
            _voucherRepo = voucherRepo;
            _voucherTaxRepo = voucherTaxRepo;
            _attachmentRepo = attachmentRepo;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<ExpenseVoucherResponseDto>> Handle(GetExpenseVoucherByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Fetching expense voucher with ID: {VoucherId}", request.Id);

                // 1. Get voucher with validation
                var voucher = await _voucherRepo.GetByID(request.Id);
                if (voucher == null)
                {
                    _logger.LogWarning("Voucher with ID {VoucherId} not found", request.Id);
                    return ResponseViewModel<ExpenseVoucherResponseDto>.Error(
                        "Voucher not found",
                        FinanceErrorCode.NotFound);
                }

                if (voucher.Type != VoucherType.Expense)
                {
                    _logger.LogWarning("Voucher with ID {VoucherId} is not an expense voucher. Type: {VoucherType}",
                        request.Id, voucher.Type);
                    return ResponseViewModel<ExpenseVoucherResponseDto>.Error(
                        "Requested voucher is not an expense voucher",
                        FinanceErrorCode.NotFound);
                }

                _logger.LogInformation("Found expense voucher: {VoucherCode}", voucher.Code);

                // 2. Get related data sequentially to avoid DbContext threading issues
                var taxLines = await _voucherTaxRepo
                    .Get(t => t.VoucherId == voucher.Id)
                    .ToListAsync(cancellationToken);

                var attachments = await _attachmentRepo
                    .Get(a => a.VoucherId == voucher.Id)
                    .ToListAsync(cancellationToken);

                _logger.LogInformation("Retrieved {TaxCount} tax lines and {AttachmentCount} attachments for voucher {VoucherId}",
                    taxLines.Count, attachments.Count, request.Id);

                // 3. Map to response DTO - AutoMapper will handle Source & Counterparty via mapping profile
                var response = _mapper.Map<ExpenseVoucherResponseDto>(voucher);

                // Map related collections
                response.TaxLines = _mapper.Map<List<TaxLineResponseDto>>(taxLines);
                response.Attachments = _mapper.Map<List<AttachmentResponseDto>>(attachments);

                _logger.LogInformation("Successfully retrieved expense voucher {VoucherCode} with all related data",
                    voucher.Code);

                return ResponseViewModel<ExpenseVoucherResponseDto>.Success(response,
                    "Expense voucher retrieved successfully");
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Request was cancelled while fetching expense voucher {VoucherId}", request.Id);
                return ResponseViewModel<ExpenseVoucherResponseDto>.Error(
                    "Request was cancelled",
                    FinanceErrorCode.RequestCancelled);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error occurred while fetching expense voucher {VoucherId}", request.Id);
                return ResponseViewModel<ExpenseVoucherResponseDto>.Error(
                    "An error occurred while fetching the expense voucher",
                    FinanceErrorCode.InternalServerError);
            }
        }
    }
}