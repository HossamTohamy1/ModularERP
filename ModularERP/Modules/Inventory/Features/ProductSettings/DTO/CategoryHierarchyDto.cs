namespace ModularERP.Modules.Inventory.Features.ProductSettings.DTO
{
    public class CategoryHierarchyDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid? ParentCategoryId { get; set; }
        public int Level { get; set; }
        public string DisplayName { get; set; } = string.Empty;
    }
}
