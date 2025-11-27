using System.ComponentModel.DataAnnotations;

namespace ModularERP.Modules.Purchases.Invoicing.DTO.DTO_SupplierPayments
{
    public class UpdateSupplierPaymentDto
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public Guid SupplierId { get; set; }

        public Guid? InvoiceId { get; set; }

        public Guid? PurchaseOrderId { get; set; }

        [Required]
        [StringLength(50)]
        public string PaymentType { get; set; } = string.Empty; // AgainstInvoice, Deposit, Advance


        [Required]
        public Guid PaymentMethodId { get; set; }
        [Required]
        public DateTime PaymentDate { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        public decimal? AllocatedAmount { get; set; }

        [StringLength(100)]
        public string? ReferenceNumber { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }
}