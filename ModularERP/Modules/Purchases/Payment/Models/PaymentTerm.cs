using ModularERP.Common.Models;
using System.ComponentModel.DataAnnotations;

namespace ModularERP.Modules.Purchases.Payment.Models
{
    public class PaymentTerm : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty; 

        [Required]
        [Range(0, 365)]
        public int Days { get; set; } // 30

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
