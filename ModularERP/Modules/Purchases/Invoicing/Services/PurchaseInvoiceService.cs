using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Enum.Purchases_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Modules.Purchases.Invoicing.DTO.DTO_Invoice;
using ModularERP.Modules.Purchases.Invoicing.Models;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Purchases.Invoicing.Services
{
    public class PurchaseInvoiceService: IPurchaseInvoiceService
    {
        private readonly IGeneralRepository<PurchaseInvoice> _invoiceRepository;
        private readonly IGeneralRepository<PurchaseOrder> _purchaseOrderRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<PurchaseInvoiceService> _logger;

        public PurchaseInvoiceService(
            IGeneralRepository<PurchaseInvoice> invoiceRepository,
            IGeneralRepository<PurchaseOrder> purchaseOrderRepository,
            IMapper mapper,
            ILogger<PurchaseInvoiceService> logger)
        {
            _invoiceRepository = invoiceRepository;
            _purchaseOrderRepository = purchaseOrderRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<string> GenerateInvoiceNumberAsync(
            Guid companyId,
            CancellationToken cancellationToken)
        {
            var year = DateTime.UtcNow.Year;
            var month = DateTime.UtcNow.Month;

            var lastInvoice = await _invoiceRepository
                .GetByCompanyId(companyId)
                .Where(i => i.CreatedAt.Year == year && i.CreatedAt.Month == month)
                .OrderByDescending(i => i.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            int sequence = 1;

            if (lastInvoice != null && !string.IsNullOrEmpty(lastInvoice.InvoiceNumber))
            {
                var parts = lastInvoice.InvoiceNumber.Split('-');
                if (parts.Length == 3 && int.TryParse(parts[2], out int lastSequence))
                {
                    sequence = lastSequence + 1;
                }
            }

            return $"INV-{year}{month:D2}-{sequence:D4}";
        }

        public async Task ValidatePurchaseOrderAsync(
            Guid purchaseOrderId,
            CancellationToken cancellationToken)
        {
            var purchaseOrder = await _purchaseOrderRepository
                .Get(po => po.Id == purchaseOrderId)
                .FirstOrDefaultAsync(cancellationToken);

            if (purchaseOrder == null)
            {
                _logger.LogWarning("Purchase order not found: {POId}", purchaseOrderId);
                throw new NotFoundException(
                    $"Purchase order with ID {purchaseOrderId} not found",
                    FinanceErrorCode.NotFound);
            }

            if (purchaseOrder.DocumentStatus != DocumentStatus.Approved)
            {
                _logger.LogWarning(
                    "Purchase order {POId} is not approved. Status: {Status}",
                    purchaseOrderId, purchaseOrder.DocumentStatus);
                throw new BusinessLogicException(
                    "Purchase order must be approved before creating an invoice",
                    "Purchases",
                    FinanceErrorCode.BusinessLogicError);
            }
        }

        public async Task UpdatePurchaseOrderPaymentStatusAsync(
            Guid purchaseOrderId,
            CancellationToken cancellationToken)
        {
            var purchaseOrder = await _purchaseOrderRepository
                .GetByIDWithTracking(purchaseOrderId);

            if (purchaseOrder == null)
            {
                return;
            }

            // Get all invoices for this PO
            var invoices = await _invoiceRepository
                .Get(i => i.PurchaseOrderId == purchaseOrderId)
                .ToListAsync(cancellationToken);

            if (!invoices.Any())
            {
                purchaseOrder.PaymentStatus = PaymentStatus.Unpaid;
            }
            else
            {
                var totalInvoiced = invoices.Sum(i => i.TotalAmount);
                var totalPaid = invoices.Sum(i => i.TotalAmount - i.AmountDue);

                if (totalPaid >= totalInvoiced)
                {
                    purchaseOrder.PaymentStatus = PaymentStatus.PaidInFull;
                }
                else if (totalPaid > 0)
                {
                    purchaseOrder.PaymentStatus = PaymentStatus.PartiallyPaid;
                }
                else
                {
                    purchaseOrder.PaymentStatus = PaymentStatus.Unpaid;
                }
            }

            await _purchaseOrderRepository.Update(purchaseOrder);
            await _purchaseOrderRepository.SaveChanges();

            _logger.LogInformation(
                "Updated PO {POId} payment status to: {Status}",
                purchaseOrderId, purchaseOrder.PaymentStatus);
        }

        public async Task<PurchaseInvoiceDto?> GetInvoiceByIdAsync(
            Guid invoiceId,
            CancellationToken cancellationToken)
        {
            return await _invoiceRepository
                .Get(i => i.Id == invoiceId)
                .ProjectTo<PurchaseInvoiceDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
