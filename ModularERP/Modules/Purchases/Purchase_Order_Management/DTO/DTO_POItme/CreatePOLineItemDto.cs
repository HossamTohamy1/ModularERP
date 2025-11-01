using System.ComponentModel.DataAnnotations;

namespace ModularERP.Modules.Purchases.Purchase_Order_Management.DTO.DTO_POItme
{
    public class CreatePOLineItemDto
    {
        [Required(ErrorMessage = "Purchase Order ID is required")]
        public Guid PurchaseOrderId { get; set; }

        public Guid? ProductId { get; set; }

        public Guid? ServiceId { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Quantity is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public decimal Quantity { get; set; }

        [Required(ErrorMessage = "Unit Price is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Unit Price cannot be negative")]
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Discount as percentage (0-100). Takes priority over DiscountAmount.
        /// If provided, DiscountAmount will be calculated automatically.
        /// </summary>
        [Range(0, 100, ErrorMessage = "Discount Percent must be between 0 and 100")]
        public decimal DiscountPercent { get; set; }

        /// <summary>
        /// Fixed discount amount. Used only if DiscountPercent is 0.
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "Discount Amount cannot be negative")]
        public decimal DiscountAmount { get; set; }

        /// <summary>
        /// Tax Profile to apply. If provided, tax will be calculated automatically.
        /// </summary>
        public Guid? TaxProfileId { get; set; }
    }
}
