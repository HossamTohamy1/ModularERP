namespace ModularERP.Modules.Inventory.Features.ProductSettings.DTO.DTO_Category
{
    public class CategoryListDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid? ParentCategoryId { get; set; }
        public string? ParentCategoryName { get; set; }
        public int SubCategoriesCount { get; set; }
        public int AttachmentsCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<CategoryAttachmentDto> Attachments { get; set; } = new List<CategoryAttachmentDto>();

    }
}
