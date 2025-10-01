namespace ModularERP.Modules.Inventory.Features.ProductSettings.DTO.DTO_Category
{
    public class CategoryResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid? ParentCategoryId { get; set; }
        public string? ParentCategoryName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<CategoryAttachmentDto> Attachments { get; set; } = new();
        public int SubCategoriesCount { get; set; }
    }
}
