using ModularERP.Common.Enum.Inventory_Enum;

namespace ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_PriceListItems
{
    public class PriceListFilterDto
    {
        public Guid? CompanyId { get; set; }
        public PriceListType? Type { get; set; }
        public PriceListStatus? Status { get; set; }
        public string? CurrencyCode { get; set; }
        public bool? IsDefault { get; set; }
        public string? SearchTerm { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
