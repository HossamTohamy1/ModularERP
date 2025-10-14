using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Warehouses.DTO.DTO_WarehouseStock;

namespace ModularERP.Modules.Inventory.Features.Warehouses.Commends.Commends_WarehouseStock
{
    public class UpdateWarehouseStockCommand : IRequest<ResponseViewModel<WarehouseStockResponseDto>>
    {
        public Guid Id { get; set; }
        public UpdateWarehouseStockDto Data { get; set; } = null!;
    }
}
