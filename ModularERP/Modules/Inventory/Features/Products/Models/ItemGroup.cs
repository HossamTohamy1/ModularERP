using ModularERP.Common.Models;
using ModularERP.Modules.Inventory.Features.ProductSettings.Models;
using System.ComponentModel.DataAnnotations;

namespace ModularERP.Modules.Inventory.Features.Products.Models
{
    public class ItemGroup : BaseEntity
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        public Guid? CategoryId { get; set; }

        public Guid? BrandId { get; set; }

        public string? Description { get; set; }

        public Guid TenantId { get; set; }
        public bool IsDeleted { get; set; }

        // Navigation Properties
        public virtual Category? Category { get; set; }
        public virtual Brand? Brand { get; set; }
        public virtual ICollection<ItemGroupItem> Items { get; set; } = new List<ItemGroupItem>();
    }
}
