using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
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
    public class UpdateSupplierPaymentHandler : IRequestHandler<UpdateSupplierPaymentCommand, ResponseViewModel<SupplierPaymentDto>>
    {
        private readonly IGeneralRepository<SupplierPayment> _paymentRepository;
        private readonly IGeneralRepository<Supplier> _supplierRepository;
        private readonly IGeneralRepository<PurchaseInvoice> _invoiceRepository;
        private readonly IGeneralRepository<PurchaseOrder> _poRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<UpdateSupplierPaymentHandler> _logger;

        public UpdateSupplierPaymentHandler(
            IGeneralRepository<SupplierPayment> paymentRepository,
            IGeneralRepository<Supplier> supplierRepository,
            IGeneralRepository<PurchaseInvoice> invoiceRepository,
            IGeneralRepository<PurchaseOrder> poRepository,
            IMapper mapper,
            ILogger<UpdateSupplierPaymentHandler> logger)
        {
            _paymentRepository = paymentRepository;
            _supplierRepository = supplierRepository;
            _invoiceRepository = invoiceRepository;
            _poRepository = poRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<SupplierPaymentDto>> Handle(
            UpdateSupplierPaymentCommand request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating supplier payment: {PaymentId}", request.Dto.Id);

            // 1. Get existing payment with related entities
            var payment = await _paymentRepository
                .Get(p => p.Id == request.Dto.Id && !p.IsDeleted)
                .Include(p => p.Invoice)
                    .ThenInclude(i => i.PurchaseOrder)
                .Include(p => p.PurchaseOrder)
                .FirstOrDefaultAsync(cancellationToken);

            if (payment == null)
            {
                _logger.LogWarning("Payment not found: {PaymentId}", request.Dto.Id);
                throw new NotFoundException(
                    $"Payment with ID {request.Dto.Id} not found",
                    Common.Enum.Finance_Enum.FinanceErrorCode.NotFound);
            }

            // 2. Check if payment is void
            if (payment.IsVoid)
            {
                _logger.LogWarning("Cannot update voided payment: {PaymentId}", request.Dto.Id);
                throw new BusinessLogicException(
                    "Cannot update a voided payment",
                    "Purchases");
            }

            // 3. Check if payment is already posted (only Draft can be updated)
            if (payment.Status != "Draft")
            {
                _logger.LogWarning("Cannot update posted payment: {PaymentId}", request.Dto.Id);
                throw new BusinessLogicException(
                    "Only draft payments can be updated",
                    "Purchases");
            }

            // Store old values for reversal if needed
            decimal oldAmount = payment.Amount;
            decimal oldAllocatedAmount = payment.AllocatedAmount;
            Guid? oldInvoiceId = payment.InvoiceId;
            Guid? oldPurchaseOrderId = payment.PurchaseOrderId;
            string oldPaymentType = payment.PaymentType;
            Guid oldSupplierId = payment.SupplierId;

            // 4. Validate PaymentType
            var validPaymentTypes = new[] { "AgainstInvoice", "Deposit", "Advance" };
            if (!validPaymentTypes.Contains(request.Dto.PaymentType))
            {
                _logger.LogWarning("Invalid PaymentType: {PaymentType}", request.Dto.PaymentType);
                throw new BusinessLogicException(
                    $"Invalid PaymentType. Allowed values: {string.Join(", ", validPaymentTypes)}",
                    "Purchases");
            }

            // 5. Validate Supplier exists
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

            PurchaseInvoice? newInvoice = null;
            PurchaseOrder? newPurchaseOrder = null;

            // 6. Validate Invoice if PaymentType = AgainstInvoice
            if (request.Dto.PaymentType == "AgainstInvoice")
            {
                if (!request.Dto.InvoiceId.HasValue)
                {
                    _logger.LogWarning("InvoiceId is required for PaymentType: AgainstInvoice");
                    throw new BusinessLogicException(
                        "Invoice ID is required when PaymentType is 'AgainstInvoice'",
                        "Purchases");
                }

                newInvoice = await _invoiceRepository
                    .Get(i => i.Id == request.Dto.InvoiceId.Value && !i.IsDeleted)
                    .Include(i => i.PurchaseOrder)
                    .FirstOrDefaultAsync(cancellationToken);

                if (newInvoice == null)
                {
                    _logger.LogWarning("Invoice not found: {InvoiceId}", request.Dto.InvoiceId);
                    throw new NotFoundException(
                        $"Invoice with ID {request.Dto.InvoiceId} not found",
                        Common.Enum.Finance_Enum.FinanceErrorCode.NotFound);
                }

                // Check if invoice belongs to supplier
                if (newInvoice.SupplierId != request.Dto.SupplierId)
                {
                    _logger.LogWarning("Invoice {InvoiceId} does not belong to Supplier {SupplierId}",
                        request.Dto.InvoiceId, request.Dto.SupplierId);
                    throw new BusinessLogicException(
                        "Invoice does not belong to the selected supplier",
                        "Purchases");
                }

                // Calculate available amount (considering old allocation reversal if same invoice)
                decimal availableAmount = newInvoice.AmountDue;
                if (oldInvoiceId == request.Dto.InvoiceId)
                {
                    // Same invoice - add back old allocated amount
                    availableAmount += oldAllocatedAmount;
                }

                // Check remaining balance
                if (availableAmount <= 0 && oldInvoiceId != request.Dto.InvoiceId)
                {
                    _logger.LogWarning("Invoice {InvoiceId} is already fully paid", request.Dto.InvoiceId);
                    throw new BusinessLogicException(
                        "Invoice is already fully paid",
                        "Purchases");
                }

                // Calculate the amount to be allocated
                decimal amountToAllocate = request.Dto.AllocatedAmount ?? request.Dto.Amount;

                // Validate payment amount doesn't exceed invoice due
                if (amountToAllocate > availableAmount)
                {
                    _logger.LogWarning(
                        "Payment amount {Amount} exceeds invoice available amount {Available}",
                        amountToAllocate, availableAmount);
                    throw new BusinessLogicException(
                        $"Payment amount ({amountToAllocate}) exceeds invoice available amount ({availableAmount})",
                        "Purchases");
                }
            }

            // 7. Validate PurchaseOrder if provided
            if (request.Dto.PurchaseOrderId.HasValue)
            {
                newPurchaseOrder = await _poRepository
                    .Get(po => po.Id == request.Dto.PurchaseOrderId.Value && !po.IsDeleted)
                    .FirstOrDefaultAsync(cancellationToken);

                if (newPurchaseOrder == null)
                {
                    _logger.LogWarning("PurchaseOrder not found: {POId}", request.Dto.PurchaseOrderId);
                    throw new NotFoundException(
                        $"Purchase Order with ID {request.Dto.PurchaseOrderId} not found",
                        Common.Enum.Finance_Enum.FinanceErrorCode.NotFound);
                }
            }

            // 8. Reverse old allocations if invoice/PO/supplier changed
            bool needToReverseOldInvoice = oldInvoiceId.HasValue &&
                                           (oldInvoiceId != request.Dto.InvoiceId ||
                                            oldSupplierId != request.Dto.SupplierId);

            if (needToReverseOldInvoice)
            {
                var oldInvoice = await _invoiceRepository
                    .Get(i => i.Id == oldInvoiceId.Value && !i.IsDeleted)
                    .Include(i => i.PurchaseOrder)
                    .FirstOrDefaultAsync(cancellationToken);

                if (oldInvoice != null)
                {
                    _logger.LogInformation("Reversing old allocation from Invoice {InvoiceId}", oldInvoiceId);

                    // Reverse old allocation
                    oldInvoice.AmountDue += oldAllocatedAmount;

                    // Update old invoice payment status
                    if (oldInvoice.AmountDue >= oldInvoice.TotalAmount)
                    {
                        oldInvoice.PaymentStatus = "Unpaid";
                    }
                    else if (oldInvoice.AmountDue > 0)
                    {
                        oldInvoice.PaymentStatus = "Partially Paid";
                    }

                    await _invoiceRepository.SaveChanges();

                    // Update old PO status if applicable
                    if (oldInvoice.PurchaseOrderId != Guid.Empty)
                    {
                        var oldPO = await _poRepository
                            .Get(p => p.Id == oldInvoice.PurchaseOrderId && !p.IsDeleted)
                            .FirstOrDefaultAsync(cancellationToken);

                        if (oldPO != null)
                        {
                            await UpdatePOPaymentStatusAsync(oldPO, cancellationToken);
                        }
                    }
                }
            }

            // 9. Update payment fields
            payment.SupplierId = request.Dto.SupplierId;
            payment.InvoiceId = request.Dto.InvoiceId;
            payment.PurchaseOrderId = request.Dto.PurchaseOrderId;
            payment.PaymentType = request.Dto.PaymentType;
            payment.PaymentMethod = request.Dto.PaymentMethod;
            payment.PaymentDate = request.Dto.PaymentDate;
            payment.Amount = request.Dto.Amount;
            payment.ReferenceNumber = request.Dto.ReferenceNumber;
            payment.Notes = request.Dto.Notes;

            // 10. Calculate Allocated/Unallocated amounts
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

            payment.UpdatedAt = DateTime.UtcNow;
            payment.UpdatedBy = Guid.Parse("f0602c31-0c12-4b5c-9ccf-fe17811d5c53").ToString();

            // 11. Save payment changes
            await _paymentRepository.SaveChanges();

            _logger.LogInformation("Payment updated successfully: {PaymentId}", payment.Id);

            // 12. Update new Invoice and PO Status (if payment is against invoice)
            if (request.Dto.PaymentType == "AgainstInvoice" && newInvoice != null)
            {
                await UpdateInvoiceAndPOStatusAsync(newInvoice, payment, cancellationToken);
            }

            // 13. Auto-apply existing deposits if applicable
            if (request.Dto.PaymentType == "AgainstInvoice" &&
                newInvoice?.PurchaseOrderId != null &&
                oldInvoiceId != request.Dto.InvoiceId) // Only if invoice changed
            {
                await ApplyExistingDepositsAsync(newInvoice, cancellationToken);
            }

            // 14. Return DTO with projection
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
                    PaymentType = p.PaymentType,
                    PaymentMethod = p.PaymentMethod,
                    PaymentDate = p.PaymentDate,
                    Amount = p.Amount,
                    AllocatedAmount = p.AllocatedAmount,
                    UnallocatedAmount = p.UnallocatedAmount,
                    ReferenceNumber = p.ReferenceNumber,
                    Notes = p.Notes,
                    Status = p.Status,
                    IsVoid = p.IsVoid,
                    VoidedAt = p.VoidedAt,
                    VoidedByName = p.VoidedByUser != null ? p.VoidedByUser.UserName : null,
                    VoidReason = p.VoidReason,
                    CreatedAt = p.CreatedAt,
                    CreatedBy = p.CreatedBy ?? ""
                })
                .FirstOrDefaultAsync(cancellationToken);

            return ResponseViewModel<SupplierPaymentDto>.Success(
                result!,
                "Payment updated successfully");
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
                invoice.PaymentStatus = "Paid in Full";
                _logger.LogInformation("Invoice {InvoiceId} marked as Paid in Full", invoice.Id);
            }
            else
            {
                var totalPaid = invoice.TotalAmount - invoice.AmountDue;
                invoice.PaymentStatus = totalPaid > 0 ? "Partially Paid" : "Unpaid";
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
                         (p.PaymentType == "Deposit" || p.PaymentType == "Advance"))
                .ToListAsync(cancellationToken);

            // Calculate total amounts
            decimal totalInvoiceAmount = invoices.Sum(i => i.TotalAmount);
            decimal totalAmountDue = invoices.Sum(i => i.AmountDue);
            decimal totalDirectPayments = directPayments.Sum(p => p.Amount);

            // Determine Payment Status
            if (totalAmountDue <= 0 && totalInvoiceAmount > 0)
            {
                po.PaymentStatus = "Paid in Full";
                _logger.LogInformation("PO {POId} marked as Paid in Full", po.Id);
            }
            else if (totalAmountDue < totalInvoiceAmount || totalDirectPayments > 0)
            {
                po.PaymentStatus = "Partially Paid";
                _logger.LogInformation("PO {POId} marked as Partially Paid", po.Id);
            }
            else
            {
                po.PaymentStatus = "Unpaid";
                _logger.LogInformation("PO {POId} remains Unpaid", po.Id);
            }

            // Check if PO should be closed
            if (po.ReceptionStatus == "Fully Received" && po.PaymentStatus == "Paid in Full")
            {
                po.DocumentStatus = "Closed";
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
                         (p.PaymentType == "Deposit" || p.PaymentType == "Advance") &&
                         p.UnallocatedAmount > 0 &&
                         !p.IsVoid)
                .OrderBy(p => p.PaymentDate)
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
                invoice.PaymentStatus = "Paid in Full";
            }
            else
            {
                invoice.PaymentStatus = invoice.AmountDue < invoice.TotalAmount
                    ? "Partially Paid"
                    : "Unpaid";
            }

            await _paymentRepository.SaveChanges();
            await _invoiceRepository.SaveChanges();

            _logger.LogInformation(
                "Deposits applied successfully. Invoice {InvoiceId} remaining due: {AmountDue}",
                invoice.Id, invoice.AmountDue);
        }
    }
}