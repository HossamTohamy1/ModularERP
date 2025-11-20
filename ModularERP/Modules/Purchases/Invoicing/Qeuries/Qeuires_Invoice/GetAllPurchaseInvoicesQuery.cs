using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Invoicing.DTO.DTO_Invoice;

namespace ModularERP.Modules.Purchases.Invoicing.Qeuries.Qeuires_Invoice
{
    public class GetAllPurchaseInvoicesQuery : IRequest<ResponseViewModel<List<PurchaseInvoiceDto>>>
    {
        public Guid CompanyId { get; set; }
        public string? SearchTerm { get; set; }
        public string? PaymentStatus { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
