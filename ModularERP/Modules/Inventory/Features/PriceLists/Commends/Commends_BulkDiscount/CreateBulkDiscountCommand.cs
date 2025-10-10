using MediatR;
using ModularERP.Common.Enum.Inventory_Enum;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_BulkDiscount;
using System.Text.Json.Serialization;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Commends.Commends_BulkDiscount
{
    public class CreateBulkDiscountCommand : IRequest<ResponseViewModel<BulkDiscountDto>>
    {
        [JsonIgnore]
        public Guid PriceListId { get; set; }
        public Guid ProductId { get; set; }
        public decimal MinQty { get; set; }
        public decimal? MaxQty { get; set; }
        public DiscountType DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
    }
}
