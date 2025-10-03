namespace ModularERP.Modules.Inventory.Features.Products.DTO.DTO_ItemGroup
{
    public class ItemGroupDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public Guid? BrandId { get; set; }
        public string? BrandName { get; set; }
        public string? Description { get; set; }
        public int ItemsCount { get; set; }
    }
}
