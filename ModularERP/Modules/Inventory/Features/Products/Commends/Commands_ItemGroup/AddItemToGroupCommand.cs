using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Products.DTO.DTO_ItemGroup;

namespace ModularERP.Modules.Inventory.Features.Products.Commends.Commands_ItemGroup
{
    public class AddItemToGroupCommand : IRequest<ResponseViewModel<ItemGroupItemDto>>
    {
        public Guid GroupId { get; set; }
        public Guid ProductId { get; set; }
        public string? SKU { get; set; }
        public decimal? PurchasePrice { get; set; }
        public decimal? SellingPrice { get; set; }
        public string? Barcode { get; set; }
    }
}
