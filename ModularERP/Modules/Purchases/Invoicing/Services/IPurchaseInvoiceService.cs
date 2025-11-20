using ModularERP.Modules.Purchases.Invoicing.DTO.DTO_Invoice;

namespace ModularERP.Modules.Purchases.Invoicing.Services
{
    public interface IPurchaseInvoiceService
    {
        Task<string> GenerateInvoiceNumberAsync(Guid companyId, CancellationToken cancellationToken);
        Task ValidatePurchaseOrderAsync(Guid purchaseOrderId, CancellationToken cancellationToken);
        Task UpdatePurchaseOrderPaymentStatusAsync(Guid purchaseOrderId, CancellationToken cancellationToken);
        Task<PurchaseInvoiceDto?> GetInvoiceByIdAsync(Guid invoiceId, CancellationToken cancellationToken);
    }
}
