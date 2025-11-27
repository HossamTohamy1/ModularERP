using ModularERP.Common.Enum.Purchases_Enum;

namespace ModularERP.Modules.Purchases.Invoicing.DTO.DTO_InvocieItem
{
    public class GetInvoicePaymentsResponse
    {
        public Guid InvoiceId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal AmountDue { get; set; }
        public List<PaymentDto> Payments { get; set; } = new();
        public int PaymentCount => Payments.Count;
        public DateTime? LastPaymentDate { get; set; }
        public PaymentStatus PaymentStatus { get; set; } // "Paid", "Partial", "Unpaid"
        public Dictionary<string, decimal> PaymentMethodBreakdown { get; set; }=new();
    }
}
