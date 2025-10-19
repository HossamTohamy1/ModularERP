using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Stocktaking.DTO.DTO_StockTaking_Header;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Commends.Commends_StockTaking_Header
{
    public class UpdateStocktakingCommand : IRequest<ResponseViewModel<UpdateStocktakingDto>>
    {
        public Guid Id { get; init; }
        public Guid WarehouseId { get; init; }
        public string Number { get; init; } = string.Empty;
        public DateTime DateTime { get; init; }
        public string? Notes { get; init; }
        public bool UpdateSystem { get; init; }
    }
}