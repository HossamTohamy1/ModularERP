using ModularERP.Common.Enum.Inventory_Enum;
using System.ComponentModel.DataAnnotations;

namespace ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_PriceListItems
{
    public class CreatePriceListDto
    {
        [Required]
        public Guid CompanyId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public PriceListType Type { get; set; }

        [Required]
        [MaxLength(3)]
        public string CurrencyCode { get; set; } = string.Empty;

        public DateTime? ValidFrom { get; set; }

        public DateTime? ValidTo { get; set; }

        public bool IsDefault { get; set; } = false;

        public PriceListStatus Status { get; set; } = PriceListStatus.Active;
    }
}
