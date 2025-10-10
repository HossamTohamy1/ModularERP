using ModularERP.Common.Enum.Inventory_Enum;

namespace ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_PriceListAssignment
{
    public class PriceListAssignmentDto
    {
        public Guid Id { get; set; }
        public PriceListEntityType EntityType { get; set; }
        public Guid EntityId { get; set; }
        public Guid PriceListId { get; set; }
        public string PriceListName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
