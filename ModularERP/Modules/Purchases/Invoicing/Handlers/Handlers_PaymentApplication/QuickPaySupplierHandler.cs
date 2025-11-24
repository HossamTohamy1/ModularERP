using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Invoicing.Commends.Commands_PaymentApplication;
using ModularERP.Modules.Purchases.Invoicing.DTO.DTO_PaymentApplication;
using ModularERP.Modules.Purchases.Invoicing.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Invoicing.Handlers.Handlers_PaymentApplication
{
    public class QuickPaySupplierHandler : IRequestHandler<QuickPaySupplierCommand, ResponseViewModel<PaymentApplicationSummaryDto>>
    {
        private readonly IGeneralRepository<SupplierPayment> _paymentRepository;
        private readonly IGeneralRepository<PaymentAllocation> _allocationRepository;
        private readonly IGeneralRepository<PurchaseInvoice> _invoiceRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<QuickPaySupplierHandler> _logger;

        public QuickPaySupplierHandler(
            IGeneralRepository<SupplierPayment> paymentRepository,
            IGeneralRepository<PaymentAllocation> allocationRepository,
            IGeneralRepository<PurchaseInvoice> invoiceRepository,
            IMapper mapper,
            ILogger<QuickPaySupplierHandler> logger)
        {
            _paymentRepository = paymentRepository;
            _allocationRepository = allocationRepository;
            _invoiceRepository = invoiceRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<PaymentApplicationSummaryDto>> Handle(
            QuickPaySupplierCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Processing quick payment for supplier {SupplierId}", request.SupplierId);

                // 1️⃣ Create payment and save first
                var payment = _mapper.Map<SupplierPayment>(request.Dto);
                payment.PaymentNumber = await GeneratePaymentNumber();

                await _paymentRepository.AddAsync(payment);
                await _paymentRepository.SaveChanges(); // 🔹 Save payment first to generate PaymentId

                var allocations = new List<PaymentAllocationResponseDto>();

                // 2️⃣ Process allocations if payment is against invoices
                if (request.Dto.PaymentType == "AgainstInvoice" && request.Dto.InvoiceAllocations?.Any() == true)
                {
                    foreach (var allocationDto in request.Dto.InvoiceAllocations)
                    {
                        var invoice = await _invoiceRepository.GetByID(allocationDto.InvoiceId);
                        if (invoice == null)
                        {
                            throw new NotFoundException($"Invoice {allocationDto.InvoiceId} not found", FinanceErrorCode.NotFound);
                        }

                        if (invoice.SupplierId != request.SupplierId)
                        {
                            throw new BusinessLogicException(
                                "Invoice does not belong to the specified supplier",
                                "Purchases",
                                FinanceErrorCode.BusinessLogicError);
                        }

                        var allocation = _mapper.Map<PaymentAllocation>(allocationDto);
                        allocation.PaymentId = payment.Id;
                        allocation.InvoiceId = invoice.Id;
                        allocation.AllocationDate = DateTime.UtcNow;

                        await _allocationRepository.AddAsync(allocation);

                        // Update invoice
                        invoice.AmountDue -= allocationDto.AllocatedAmount;
                        invoice.PaymentStatus = invoice.AmountDue == 0 ? "PaidInFull" : "PartiallyPaid";

                        allocations.Add(new PaymentAllocationResponseDto
                        {
                            Id = allocation.Id,
                            PaymentId = payment.Id,
                            PaymentNumber = payment.PaymentNumber,
                            InvoiceId = invoice.Id,
                            InvoiceNumber = invoice.InvoiceNumber,
                            AllocatedAmount = allocationDto.AllocatedAmount,
                            AllocationDate = allocation.AllocationDate,
                            Notes = allocation.Notes
                        });
                    }

                    // Save allocations and invoice updates
                    await _allocationRepository.SaveChanges();
                    await _invoiceRepository.SaveChanges();
                }

                _logger.LogInformation(
                    "Quick payment {PaymentNumber} created for supplier {SupplierId}, Amount: {Amount}",
                    payment.PaymentNumber, request.SupplierId, request.Dto.Amount);

                // Return summary
                var summary = _mapper.Map<PaymentApplicationSummaryDto>(payment);
                summary.Allocations = allocations;
                summary.AllocationsCount = allocations.Count; 


                return ResponseViewModel<PaymentApplicationSummaryDto>.Success(
                    summary,
                    "Payment processed successfully");
            }
            catch (BaseApplicationException ex)
            {
                _logger.LogError(ex, "Error processing quick payment");
                return ResponseViewModel<PaymentApplicationSummaryDto>.Fail(
                    ex.Message,
                    ex.FinanceErrorCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error processing quick payment");
                return ResponseViewModel<PaymentApplicationSummaryDto>.Fail(
                    $"An unexpected error occurred: {ex.Message}",
                    FinanceErrorCode.InternalServerError);
            }
        }

        private async Task<string> GeneratePaymentNumber()
        {
            var lastPayment = await _paymentRepository
                .GetAll()
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefaultAsync();

            if (lastPayment == null || string.IsNullOrEmpty(lastPayment.PaymentNumber))
                return "PAY-00001";

            var parts = lastPayment.PaymentNumber.Split('-');
            if (parts.Length != 2 || !int.TryParse(parts[1], out int lastNumber))
                return "PAY-00001";

            return $"PAY-{(lastNumber + 1):D5}";
        }
    }
}
