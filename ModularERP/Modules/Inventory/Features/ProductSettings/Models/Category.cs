using ModularERP.Common.Models;
using System.ComponentModel.DataAnnotations;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.Models
{
    public class Category : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        // Self-referencing for hierarchy (Parent Category)
        public Guid? ParentCategoryId { get; set; }
        public virtual Category? ParentCategory { get; set; }

        // Navigation properties
        public virtual ICollection<Category> SubCategories { get; set; } = new List<Category>();
        public virtual ICollection<CategoryAttachment> Attachments { get; set; } = new List<CategoryAttachment>();
    }
}
