using ModularERP.Common.Enum.Inventory_Enum;

namespace ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_PriceListAssignment
{
    public class CreatePriceListAssignmentDto
    {
        public PriceListEntityType EntityType { get; set; }
        public Guid EntityId { get; set; }
        public Guid PriceListId { get; set; }
    }
}
