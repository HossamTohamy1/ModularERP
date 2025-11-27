using ModularERP.Common.Enum.Purchases_Enum;
using System.ComponentModel.DataAnnotations;

namespace ModularERP.Modules.Purchases.Purchase_Order_Management.DTO.DTO_PurchaseOrder
{
    public class UpdatePODepositDto
    {
        public Guid? Id { get; set; }
        public decimal Amount { get; set; }
        public decimal? Percentage { get; set; }
        [Required]
        public Guid PaymentMethodId { get; set; }
        public string? ReferenceNumber { get; set; }
        public bool AlreadyPaid { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string? Notes { get; set; }
    }
}
