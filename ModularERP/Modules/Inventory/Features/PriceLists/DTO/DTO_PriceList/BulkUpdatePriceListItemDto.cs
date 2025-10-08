using System.ComponentModel.DataAnnotations;

namespace ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_PriceList
{
    public class BulkUpdatePriceListItemDto
    {
        [Required]
        public List<BulkUpdateItemDto> Items { get; set; } = new();
    }
}
