using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Purchases_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Suppliers.Models;
using ModularERP.Modules.Purchases.Invoicing.Commends.Commands_SupplierPayments;
using ModularERP.Modules.Purchases.Invoicing.DTO.DTO_SupplierPayments;
using ModularERP.Modules.Purchases.Invoicing.Models;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Invoicing.Handlers.Handlers_SupplierPayments
{
    public class CreateSupplierPaymentHandler : IRequestHandler<CreateSupplierPaymentCommand, ResponseViewModel<SupplierPaymentDto>>
    {
        private readonly IGeneralRepository<SupplierPayment> _paymentRepository;
        private readonly IGeneralRepository<Supplier> _supplierRepository;
        private readonly IGeneralRepository<PurchaseInvoice> _invoiceRepository;
        private readonly IGeneralRepository<PurchaseOrder> _poRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateSupplierPaymentHandler> _logger;

        public CreateSupplierPaymentHandler(
            IGeneralRepository<SupplierPayment> paymentRepository,
            IGeneralRepository<Supplier> supplierRepository,
            IGeneralRepository<PurchaseInvoice> invoiceRepository,
            IGeneralRepository<PurchaseOrder> poRepository,
            IMapper mapper,
            ILogger<CreateSupplierPaymentHandler> logger)
        {
            _paymentRepository = paymentRepository;
            _supplierRepository = supplierRepository;
            _invoiceRepository = invoiceRepository;
            _poRepository = poRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<SupplierPaymentDto>> Handle(
            CreateSupplierPaymentCommand request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating supplier payment for Supplier: {SupplierId}", request.Dto.SupplierId);

            // 1. Validate PaymentType
            var validPaymentTypes = new[] { "AgainstInvoice", "Deposit", "Advance" };
            if (!validPaymentTypes.Contains(request.Dto.PaymentType))
            {
                _logger.LogWarning("Invalid PaymentType: {PaymentType}", request.Dto.PaymentType);
                throw new BusinessLogicException(
                    $"Invalid PaymentType. Allowed values: {string.Join(", ", validPaymentTypes)}",
                    "Purchases");
            }

            // 2. Validate Supplier exists
            var supplierExists = await _supplierRepository.AnyAsync(
                s => s.Id == request.Dto.SupplierId && !s.IsDeleted,
                cancellationToken);

            if (!supplierExists)
            {
                _logger.LogWarning("Supplier not found: {SupplierId}", request.Dto.SupplierId);
                throw new NotFoundException(
                    $"Supplier with ID {request.Dto.SupplierId} not found",
                    Common.Enum.Finance_Enum.FinanceErrorCode.NotFound);
            }

            PurchaseInvoice? invoice = null;
            PurchaseOrder? purchaseOrder = null;

            // 3. Validate Invoice if PaymentType = AgainstInvoice
            if (request.Dto.PaymentType == "AgainstInvoice")
            {
                if (!request.Dto.InvoiceId.HasValue)
                {
                    _logger.LogWarning("InvoiceId is required for PaymentType: AgainstInvoice");
                    throw new BusinessLogicException(
                        "Invoice ID is required when PaymentType is 'AgainstInvoice'",
                        "Purchases");
                }

                invoice = await _invoiceRepository
                    .Get(i => i.Id == request.Dto.InvoiceId.Value && !i.IsDeleted)
                    .Include(i => i.PurchaseOrder)
                    .FirstOrDefaultAsync(cancellationToken);

                if (invoice == null)
                {
                    _logger.LogWarning("Invoice not found: {InvoiceId}", request.Dto.InvoiceId);
                    throw new NotFoundException(
                        $"Invoice with ID {request.Dto.InvoiceId} not found",
                        Common.Enum.Finance_Enum.FinanceErrorCode.NotFound);
                }

                // Check if invoice belongs to supplier
                if (invoice.SupplierId != request.Dto.SupplierId)
                {
                    _logger.LogWarning("Invoice {InvoiceId} does not belong to Supplier {SupplierId}",
                        request.Dto.InvoiceId, request.Dto.SupplierId);
                    throw new BusinessLogicException(
                        "Invoice does not belong to the selected supplier",
                        "Purchases");
                }

                // Check remaining balance
                if (invoice.AmountDue <= 0)
                {
                    _logger.LogWarning("Invoice {InvoiceId} is already fully paid", request.Dto.InvoiceId);
                    throw new BusinessLogicException(
                        "Invoice is already fully paid",
                        "Purchases");
                }

                // Validate payment amount doesn't exceed invoice due
                if (request.Dto.Amount > invoice.AmountDue)
                {
                    _logger.LogWarning(
                        "Payment amount {Amount} exceeds invoice due amount {AmountDue}",
                        request.Dto.Amount, invoice.AmountDue);
                    throw new BusinessLogicException(
                        $"Payment amount ({request.Dto.Amount}) exceeds invoice due amount ({invoice.AmountDue})",
                        "Purchases");
                }
            }

            // 4. Validate PurchaseOrder if provided
            if (request.Dto.PurchaseOrderId.HasValue)
            {
                purchaseOrder = await _poRepository
                    .Get(po => po.Id == request.Dto.PurchaseOrderId.Value && !po.IsDeleted)
                    .FirstOrDefaultAsync(cancellationToken);

                if (purchaseOrder == null)
                {
                    _logger.LogWarning("PurchaseOrder not found: {POId}", request.Dto.PurchaseOrderId);
                    throw new NotFoundException(
                        $"Purchase Order with ID {request.Dto.PurchaseOrderId} not found",
                        Common.Enum.Finance_Enum.FinanceErrorCode.NotFound);
                }
            }

            // 5. Map DTO to Entity
            var payment = _mapper.Map<SupplierPayment>(request.Dto);

            // 6. Generate Payment Number
            payment.PaymentNumber = await GeneratePaymentNumberAsync(cancellationToken);

            // 7. Calculate Allocated/Unallocated amounts
            if (request.Dto.AllocatedAmount.HasValue)
            {
                // Validate allocated amount
                if (request.Dto.AllocatedAmount.Value > request.Dto.Amount)
                {
                    throw new BusinessLogicException(
                        "Allocated amount cannot exceed total payment amount",
                        "Purchases");
                }

                payment.AllocatedAmount = request.Dto.AllocatedAmount.Value;
                payment.UnallocatedAmount = payment.Amount - payment.AllocatedAmount;
            }
            else
            {
                // Auto-allocate full amount if against invoice, otherwise 0
                payment.AllocatedAmount = request.Dto.PaymentType == "AgainstInvoice"
                    ? payment.Amount
                    : 0;
                payment.UnallocatedAmount = payment.Amount - payment.AllocatedAmount;
            }

            payment.Status = SupplierPaymentStatus.Draft;
            payment.CreatedBy = Guid.Parse("f0602c31-0c12-4b5c-9ccf-fe17811d5c53").ToString();
            payment.CreatedAt = DateTime.UtcNow;

            // 8. Save Payment
            await _paymentRepository.AddAsync(payment);
            await _paymentRepository.SaveChanges();

            _logger.LogInformation("Supplier payment created successfully: {PaymentId}", payment.Id);

            // 9. Update Invoice and PO Status (if payment is against invoice)
            if (request.Dto.PaymentType == "AgainstInvoice" && invoice != null)
            {
                await UpdateInvoiceAndPOStatusAsync(invoice, payment, cancellationToken);
            }

            // 10. Auto-apply existing deposits if applicable
            if (request.Dto.PaymentType == "AgainstInvoice" && invoice?.PurchaseOrderId != null)
            {
                await ApplyExistingDepositsAsync(invoice, cancellationToken);
            }

            // 11. Return DTO with projection
            var result = await _paymentRepository
                .Get(p => p.Id == payment.Id)
                .Select(p => new SupplierPaymentDto
                {
                    Id = p.Id,
                    SupplierId = p.SupplierId,
                    SupplierName = p.Supplier.Name,
                    InvoiceId = p.InvoiceId,
                    InvoiceNumber = p.Invoice != null ? p.Invoice.InvoiceNumber : null,
                    PurchaseOrderId = p.PurchaseOrderId,
                    PONumber = p.PurchaseOrder != null ? p.PurchaseOrder.PONumber : null,
                    PaymentNumber = p.PaymentNumber,
                    PaymentType = p.PaymentType.ToString(),
                    PaymentMethodId = p.PaymentMethodId,
                    PaymentDate = p.PaymentDate,
                    Amount = p.Amount,
                    AllocatedAmount = p.AllocatedAmount,
                    UnallocatedAmount = p.UnallocatedAmount,
                    ReferenceNumber = p.ReferenceNumber,
                    Notes = p.Notes,
                    Status = p.Status.ToString(),
                    IsVoid = p.IsVoid,
                    VoidedAt = p.VoidedAt,
                    VoidedByName = p.VoidedByUser != null ? p.VoidedByUser.UserName : null,
                    VoidReason = p.VoidReason,
                    CreatedAt = p.CreatedAt,
                    CreatedBy = p.CreatedBy ?? "",
                })
                .FirstOrDefaultAsync(cancellationToken);

            return ResponseViewModel<SupplierPaymentDto>.Success(
                result!,
                "Payment created successfully");
        }

        /// <summary>
        /// Updates Invoice AmountDue and Payment Status, and cascades to PO if applicable
        /// </summary>
        private async Task UpdateInvoiceAndPOStatusAsync(
            PurchaseInvoice invoice,
            SupplierPayment payment,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Updating Invoice {InvoiceId} status after payment {PaymentId}",
                invoice.Id, payment.Id);

            // Update Invoice AmountDue
            invoice.AmountDue -= payment.AllocatedAmount;

            // Ensure AmountDue doesn't go negative
            if (invoice.AmountDue < 0)
            {
                invoice.AmountDue = 0;
            }

            // Update Invoice Payment Status
            if (invoice.AmountDue <= 0)
            {
                invoice.PaymentStatus = PaymentStatus.PaidInFull;
                _logger.LogInformation("Invoice {InvoiceId} marked as Paid in Full", invoice.Id);
            }
            else
            {
                // Check if there are any payments (not just this one)
                var totalPaid = invoice.TotalAmount - invoice.AmountDue;
                invoice.PaymentStatus = totalPaid > 0 ? PaymentStatus.PartiallyPaid : PaymentStatus.Unpaid;
                _logger.LogInformation(
                    "Invoice {InvoiceId} status: {Status}, Amount Due: {AmountDue}",
                    invoice.Id, invoice.PaymentStatus, invoice.AmountDue);
            }

            await _invoiceRepository.SaveChanges();

            // Update PO Payment Status if invoice is linked to PO
            if (invoice.PurchaseOrderId != Guid.Empty)
            {
                var po = await _poRepository
                    .Get(p => p.Id == invoice.PurchaseOrderId && !p.IsDeleted)
                    .FirstOrDefaultAsync(cancellationToken);

                if (po != null)
                {
                    await UpdatePOPaymentStatusAsync(po, cancellationToken);
                }
            }
        }

        /// <summary>
        /// Updates PO Payment Status based on all invoices and payments
        /// </summary>
        private async Task UpdatePOPaymentStatusAsync(
            PurchaseOrder po,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating PO {POId} payment status", po.Id);

            // Get all invoices linked to this PO
            var invoices = await _invoiceRepository
                .Get(i => i.PurchaseOrderId == po.Id && !i.IsDeleted)
                .ToListAsync(cancellationToken);

            // Get all direct payments to PO (deposits/advances)
            var directPayments = await _paymentRepository
                .Get(p => p.PurchaseOrderId == po.Id &&
                         !p.IsVoid &&
                         (p.PaymentType == PaymentType.Deposit || p.PaymentType == PaymentType.Advance))
                .ToListAsync(cancellationToken);

            // Calculate total amounts
            decimal totalInvoiceAmount = invoices.Sum(i => i.TotalAmount);
            decimal totalAmountDue = invoices.Sum(i => i.AmountDue);
            decimal totalDirectPayments = directPayments.Sum(p => p.Amount);

            // Determine Payment Status
            if (totalAmountDue <= 0 && totalInvoiceAmount > 0)
            {
                po.PaymentStatus = PaymentStatus.PaidInFull;
                _logger.LogInformation("PO {POId} marked as Paid in Full", po.Id);
            }
            else if (totalAmountDue < totalInvoiceAmount || totalDirectPayments > 0)
            {
                po.PaymentStatus = PaymentStatus.PartiallyPaid;
                _logger.LogInformation("PO {POId} marked as Partially Paid", po.Id);
            }
            else
            {
                po.PaymentStatus = PaymentStatus.Unpaid;
                _logger.LogInformation("PO {POId} remains Unpaid", po.Id);
            }

            // Check if PO should be closed
            // PO can be closed if: Reception = Fully Received AND Payment = Paid in Full
            if (po.ReceptionStatus == ReceptionStatus.FullyReceived && po.PaymentStatus == PaymentStatus.PaidInFull)
            {
                po.DocumentStatus = DocumentStatus.Closed;
                _logger.LogInformation(
                    "PO {POId} auto-closed (Reception: {Reception}, Payment: {Payment})",
                    po.Id, po.ReceptionStatus, po.PaymentStatus);
            }

            await _poRepository.SaveChanges();
        }

        /// <summary>
        /// Auto-apply existing unallocated deposits to the invoice
        /// </summary>
        private async Task ApplyExistingDepositsAsync(
            PurchaseInvoice invoice,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Checking for existing deposits for PO {POId}",
                invoice.PurchaseOrderId);

            // Get unallocated deposits for this PO
            var existingDeposits = await _paymentRepository
                .Get(p => p.PurchaseOrderId == invoice.PurchaseOrderId &&
                         (p.PaymentType == PaymentType.Deposit || p.PaymentType == PaymentType.Advance) &&
                         p.UnallocatedAmount > 0 &&
                         !p.IsVoid)
                .OrderBy(p => p.PaymentDate) // Apply oldest deposits first
                .ToListAsync(cancellationToken);

            if (!existingDeposits.Any())
            {
                _logger.LogInformation("No unallocated deposits found for PO {POId}", invoice.PurchaseOrderId);
                return;
            }

            decimal remainingDue = invoice.AmountDue;

            foreach (var deposit in existingDeposits)
            {
                if (remainingDue <= 0)
                    break;

                // Calculate amount to apply from this deposit
                decimal amountToApply = Math.Min(deposit.UnallocatedAmount, remainingDue);

                // Update deposit allocation
                deposit.AllocatedAmount += amountToApply;
                deposit.UnallocatedAmount -= amountToApply;

                // Update invoice
                invoice.AmountDue -= amountToApply;
                remainingDue -= amountToApply;

                _logger.LogInformation(
                    "Applied {Amount} from deposit {DepositId} to invoice {InvoiceId}",
                    amountToApply, deposit.Id, invoice.Id);
            }

            // Update payment status after applying deposits
            if (invoice.AmountDue <= 0)
            {
                invoice.PaymentStatus = PaymentStatus.PaidInFull;
            }
            else
            {
                invoice.PaymentStatus = invoice.AmountDue < invoice.TotalAmount
                    ? PaymentStatus.PartiallyPaid
                    : PaymentStatus.Unpaid;
            }

            await _paymentRepository.SaveChanges();
            await _invoiceRepository.SaveChanges();

            _logger.LogInformation(
                "Deposits applied successfully. Invoice {InvoiceId} remaining due: {AmountDue}",
                invoice.Id, invoice.AmountDue);
        }

        /// <summary>
        /// Generates unique sequential payment number
        /// </summary>
        private async Task<string> GeneratePaymentNumberAsync(CancellationToken cancellationToken)
        {
            var lastPayment = await _paymentRepository
                .GetAll()
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => p.PaymentNumber)
                .FirstOrDefaultAsync(cancellationToken);

            if (string.IsNullOrEmpty(lastPayment))
            {
                return "PAY-00001";
            }

            try
            {
                var lastNumber = int.Parse(lastPayment.Split('-')[1]);
                return $"PAY-{(lastNumber + 1):D5}";
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse last payment number: {PaymentNumber}", lastPayment);
                return "PAY-00001";
            }
        }
    }
}