namespace ModularERP.Modules.Purchases.Refunds.DTO.DTO_RefundInvoce
{
    public class POStatusDto
    {
        public string ReceptionStatus { get; set; }
        public string PaymentStatus { get; set; }
        public string DocumentStatus { get; set; }
        public decimal TotalOrdered { get; set; }
        public decimal TotalReceived { get; set; }
        public decimal TotalReturned { get; set; }
        public decimal NetReceived { get; set; }
    }
}
