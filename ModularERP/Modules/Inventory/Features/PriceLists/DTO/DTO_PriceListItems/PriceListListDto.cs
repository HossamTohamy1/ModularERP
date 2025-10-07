using ModularERP.Common.Enum.Inventory_Enum;

namespace ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_PriceListItems
{
    public class PriceListListDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public PriceListType Type { get; set; }
        public string TypeName { get; set; } = string.Empty;
        public string CurrencyCode { get; set; } = string.Empty;
        public PriceListStatus Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
