using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Stocktaking.DTO.DTO_StockTaking_Line;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Qeuries.Qeuries_StockTaking_Line
{
    public class GetStocktakingLineByIdQuery : IRequest<ResponseViewModel<StocktakingLineDto>>
    {
        public Guid StocktakingId { get; set; }
        public Guid LineId { get; set; }
    }

}
