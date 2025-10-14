using MediatR;
using ModularERP.Common.ViewModel;

namespace ModularERP.Modules.Inventory.Features.Warehouses.Commends.Commends_WarehouseStock
{
    public class DeleteWarehouseStockCommand : IRequest<ResponseViewModel<bool>>
    {
        public Guid Id { get; set; }
    }
}
