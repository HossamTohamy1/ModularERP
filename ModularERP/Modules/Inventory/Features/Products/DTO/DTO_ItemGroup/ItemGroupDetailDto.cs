namespace ModularERP.Modules.Inventory.Features.Products.DTO.DTO_ItemGroup
{
    public class ItemGroupDetailDto : ItemGroupDto
    {
        public List<ItemGroupItemDto> Items { get; set; } = new();
    }
}
