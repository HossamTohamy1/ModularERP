using System.ComponentModel.DataAnnotations;

namespace ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_PriceListItems
{
    public class ClonePriceListDto
    {
        [Required]
        public Guid SourcePriceListId { get; set; }

        [Required]
        [MaxLength(200)]
        public string NewName { get; set; } = string.Empty;

        public bool CopyItems { get; set; } = true;
        public bool CopyRules { get; set; } = true;
        public bool CopyBulkDiscounts { get; set; } = true;
        public bool CopyAssignments { get; set; } = false;
    }
}
