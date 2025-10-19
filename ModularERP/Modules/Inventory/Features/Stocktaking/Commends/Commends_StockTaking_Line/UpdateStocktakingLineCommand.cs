using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Stocktaking.DTO.DTO_StockTaking_Line;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Commends.Commends_StockTaking_Line
{
    public class UpdateStocktakingLineCommand : IRequest<ResponseViewModel<StocktakingLineDto>>
    {
        public Guid StocktakingId { get; set; }
        public Guid LineId { get; set; }
        public decimal PhysicalQty { get; set; }
        public string Note { get; set; }
        public string ImagePath { get; set; }
    }

}
