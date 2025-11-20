using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Invoicing.DTO.DTO_Invoice;

namespace ModularERP.Modules.Purchases.Invoicing.Commends.Commands_Invoice
{
    public class CreatePurchaseInvoiceCommand : IRequest<ResponseViewModel<PurchaseInvoiceDto>>
    {
        public Guid PurchaseOrderId { get; set; }
        public Guid CompanyId { get; set; }
        public Guid SupplierId { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal DepositApplied { get; set; }
        public string? Notes { get; set; }
        public List<CreateInvoiceLineItemDto> LineItems { get; set; } = new();
    }
}