using System.ComponentModel.DataAnnotations;

namespace ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_PriceList
{
    public class CreatePriceListItemDto
    {
        public Guid? ProductId { get; set; }
        public Guid? ServiceId { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? BasePrice { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? ListPrice { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? DiscountValue { get; set; }

        [MaxLength(20)]
        public string? DiscountType { get; set; }

        public Guid? TaxProfileId { get; set; }

        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
    }
}
