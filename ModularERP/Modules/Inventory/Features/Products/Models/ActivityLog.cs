using ModularERP.Common.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ModularERP.Modules.Inventory.Features.Products.Models
{
    public class ActivityLog : BaseEntity
    {
        [Required]
        public Guid ProductId { get; set; }

        [Required]
        [MaxLength(100)]
        public string ActionType { get; set; } = string.Empty;

        // مثال: Create, Update, Delete, Transaction, PriceChange
        public Guid? UserId { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string? BeforeValues { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string? AfterValues { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        // Navigation Properties
        public virtual Product? Product { get; set; }
    }
}
