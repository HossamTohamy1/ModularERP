using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.Warehouses.DTO.DTO_WarehouseStock;

namespace ModularERP.Modules.Inventory.Features.Warehouses.Commends.Commends_WarehouseStock
{
    public class GetAllWarehouseStocksQuery : IRequest<ResponseViewModel<List<WarehouseStockListDto>>>
    {
        public Guid? WarehouseId { get; set; }
        public Guid? ProductId { get; set; }
        public bool? LowStockOnly { get; set; }
    }
}
