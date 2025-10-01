using MediatR;

namespace ModularERP.Modules.Inventory.Features.Warehouses.Commends
{
    public class DeleteWarehouseCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
    }
}
