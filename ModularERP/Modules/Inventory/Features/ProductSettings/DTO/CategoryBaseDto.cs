using System.ComponentModel.DataAnnotations;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.DTO
{
    public class CategoryBaseDto
    {
        [Required(ErrorMessage = "Category name is required")]
        [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        public Guid? ParentCategoryId { get; set; }
    }
}
