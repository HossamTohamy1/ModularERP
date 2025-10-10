using ModularERP.Common.Enum.Inventory_Enum;

namespace ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_BulkDiscount
{
    public class CreateBulkDiscountDto
    {
        public Guid ProductId { get; set; }
        public decimal MinQty { get; set; }
        public decimal? MaxQty { get; set; }
        public DiscountType DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
    }
}
