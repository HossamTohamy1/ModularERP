using System.ComponentModel.DataAnnotations;

namespace ModularERP.Modules.Purchases.Payment.DTO.DTO_PaymentTerm
{
    public class CreatePaymentTermDto
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Days is required")]
        [Range(0, 365, ErrorMessage = "Days must be between 0 and 365")]
        public int Days { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
