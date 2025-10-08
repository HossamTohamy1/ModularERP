using System.ComponentModel.DataAnnotations;

namespace ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_PriceList
{
    public class BulkCreatePriceListItemDto
    {
        [Required]
        public List<CreatePriceListItemDto> Items { get; set; } = new();
    }
}
