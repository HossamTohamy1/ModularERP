using ModularERP.Common.Models;
using System.ComponentModel.DataAnnotations;

namespace ModularERP.Modules.Inventory.Features.TaxManagement.Models
{
    public class TaxProfile : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool Active { get; set; } = true;

        public Guid TenantId { get; set; }
        public bool IsDeleted { get; set; }

        // Navigation Properties
        public virtual ICollection<TaxProfileComponent> TaxProfileComponents { get; set; } = new List<TaxProfileComponent>();
    }
}
