using ModularERP.Common.Enum.Purchases_Enum;

namespace ModularERP.Modules.Purchases.Invoicing.DTO.DTO_InvocieItem
{
    public class PaymentDto
    {
        public Guid Id { get; set; }
        public Guid PaymentMethodId { get; set; }

        public string PaymentMethodName { get; set; } = string.Empty;
        public string PaymentMethodCode { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; }
        public decimal Amount { get; set; }
        public string? ReferenceNumber { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
