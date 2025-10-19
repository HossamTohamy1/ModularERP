using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Stocktaking.DTO.DTO_StockTaking_Header;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Qeuries.Qeuries_StockTaking_Header
{
    public class GetAllStocktakingQuery : IRequest<ResponseViewModel<List<StocktakingListDto>>>
    {
        public Guid? CompanyId { get; init; }
        public Guid? WarehouseId { get; init; }
        public string? Status { get; init; }
    }
}