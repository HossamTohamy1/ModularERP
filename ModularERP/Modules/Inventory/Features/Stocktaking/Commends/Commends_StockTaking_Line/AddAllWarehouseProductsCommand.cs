using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Stocktaking.DTO.DTO_StockTaking_Line;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Commends.Commends_StockTaking_Line
{
    public class AddAllWarehouseProductsCommand : IRequest<ResponseViewModel<List<StocktakingLineDto>>>
    {
        public Guid StocktakingId { get; set; }
    }
}
