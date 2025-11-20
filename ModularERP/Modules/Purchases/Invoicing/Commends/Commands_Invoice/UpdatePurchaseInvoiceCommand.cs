using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Invoicing.DTO.DTO_Invoice;

namespace ModularERP.Modules.Purchases.Invoicing.Commends.Commands_Invoice
{
    public class UpdatePurchaseInvoiceCommand : IRequest<ResponseViewModel<PurchaseInvoiceDto>>
    {
        public Guid Id { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal DepositApplied { get; set; }
        public string? Notes { get; set; }
        public List<UpdateInvoiceLineItemDto> LineItems { get; set; } = new();
    }
}