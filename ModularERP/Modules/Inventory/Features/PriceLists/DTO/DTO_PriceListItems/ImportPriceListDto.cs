using System.ComponentModel.DataAnnotations;

namespace ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_PriceListItems
{
    public class ImportPriceListDto
    {
        [Required]
        public Guid CompanyId { get; set; }

        [Required]
        public IFormFile File { get; set; }

        public bool UpdateExisting { get; set; } = false;

    }
}
