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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
            _logger.LogInformation("➡️ Start Handle GetExpenseVoucherById with ID: {VoucherId}", request.Id);

            Voucher? voucher = null;
            List<VoucherTax> taxLines = new();
            List<VoucherAttachment> attachments = new();

            // 1. Get Voucher
            try
            {
                voucher = await _voucherRepo.GetByID(request.Id);
                if (voucher == null)
                {
                    _logger.LogWarning("❌ Voucher with ID {VoucherId} not found", request.Id);
                    return ResponseViewModel<ExpenseVoucherResponseDto>.Error(
                        "Voucher not found",
                        FinanceErrorCode.NotFound);
                }

                if (voucher.Type != VoucherType.Expense)
                {
                    _logger.LogWarning("❌ Voucher with ID {VoucherId} is not an expense voucher. Type: {VoucherType}",
                        request.Id, voucher.Type);
                    return ResponseViewModel<ExpenseVoucherResponseDto>.Error(
                        "Requested voucher is not an expense voucher",
                        FinanceErrorCode.NotFound);
                }

                _logger.LogInformation("✅ Voucher retrieved: {VoucherCode}", voucher.Code);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error fetching voucher with ID {VoucherId}", request.Id);
                return ResponseViewModel<ExpenseVoucherResponseDto>.Error(
                    "Error fetching voucher",
                    FinanceErrorCode.InternalServerError);
            }

            // 2. Get Tax Lines
            try
            {
                taxLines = await _voucherTaxRepo
                    .Get(t => t.VoucherId == voucher.Id)
                    .ToListAsync(cancellationToken);

                _logger.LogInformation("✅ Retrieved {Count} tax lines for voucher {VoucherId}", taxLines.Count, voucher.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error fetching tax lines for voucher {VoucherId}", voucher.Id);
                return ResponseViewModel<ExpenseVoucherResponseDto>.Error(
                    "Error fetching tax lines",
                    FinanceErrorCode.InternalServerError);
            }

            // 3. Get Attachments
            try
            {
                attachments = await _attachmentRepo
                    .Get(a => a.VoucherId == voucher.Id)
                    .ToListAsync(cancellationToken);

                _logger.LogInformation("✅ Retrieved {Count} attachments for voucher {VoucherId}", attachments.Count, voucher.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error fetching attachments for voucher {VoucherId}", voucher.Id);
                return ResponseViewModel<ExpenseVoucherResponseDto>.Error(
                    "Error fetching attachments",
                    FinanceErrorCode.InternalServerError);
            }

            // 4. Build Response
            try
            {
                var response = _mapper.Map<ExpenseVoucherResponseDto>(voucher);

                response.TaxLines = taxLines.Select(t => new TaxLineResponseDto
                {
                    TaxId = t.TaxId,
                    BaseAmount = t.BaseAmount,
                    TaxAmount = t.TaxAmount,
                    IsWithholding = t.IsWithholding
                }).ToList();

                response.Attachments = attachments.Select(a => new AttachmentResponseDto
                {
                    Id = a.Id,
                    FileName = a.Filename,
                    FileUrl = a.FilePath
                }).ToList();

                _logger.LogInformation("✅ Successfully built response for voucher {VoucherCode}", voucher.Code);

                return ResponseViewModel<ExpenseVoucherResponseDto>.Success(response,
                    "Expense voucher retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error building response for voucher {VoucherId}", voucher.Id);
                return ResponseViewModel<ExpenseVoucherResponseDto>.Error(
                    "Error building response",
                    FinanceErrorCode.InternalServerError);
            }
        }
    }
}
