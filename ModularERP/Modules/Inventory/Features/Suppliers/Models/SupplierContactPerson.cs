using ModularERP.Common.Models;
using System.ComponentModel.DataAnnotations;

namespace ModularERP.Modules.Inventory.Features.Suppliers.Models
{
    public class SupplierContactPerson : BaseEntity
    {
        [Required]
        public Guid SupplierId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Phone { get; set; }

        [MaxLength(100)]
        [EmailAddress]
        public string? Email { get; set; }

        [MaxLength(100)]
        public string? Position { get; set; }

        public bool IsPrimary { get; set; } = false; 

        // Navigation Properties
        public virtual Supplier Supplier { get; set; } = null!;
    }
}
