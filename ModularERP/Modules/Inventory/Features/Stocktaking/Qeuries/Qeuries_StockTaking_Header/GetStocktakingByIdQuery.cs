using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Stocktaking.DTO.DTO_StockTaking_Header;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Qeuries.Qeuries_StockTaking_Header
{
    public record GetStocktakingByIdQuery(Guid Id) : IRequest<ResponseViewModel<StocktakingDetailDto>>;
}
