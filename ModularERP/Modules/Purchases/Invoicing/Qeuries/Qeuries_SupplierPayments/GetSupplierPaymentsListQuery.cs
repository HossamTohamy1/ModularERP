using MediatR;
using ModularERP.Common.Models;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Invoicing.DTO.DTO_SupplierPayments;

namespace ModularERP.Modules.Purchases.Invoicing.Qeuries.Qeuries_SupplierPayments
{
    public class GetSupplierPaymentsListQuery
        : IRequest<ResponseViewModel<PagedResult<SupplierPaymentDto>>>
    {
        // Pagination
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        // Sorting
        public string? SortBy { get; set; }
        public bool SortDescending { get; set; } = true;

        // Filters
        public Guid? SupplierId { get; set; }
        public Guid? InvoiceId { get; set; }
        public Guid? PurchaseOrderId { get; set; }
        public string? Status { get; set; }
        public string? PaymentType { get; set; }
        public string? PaymentMethod { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? SearchTerm { get; set; }
    }
}