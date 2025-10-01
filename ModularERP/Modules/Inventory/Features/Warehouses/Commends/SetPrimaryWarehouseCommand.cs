using MediatR;
using ModularERP.Modules.Inventory.Features.Warehouses.DTO;

namespace ModularERP.Modules.Inventory.Features.Warehouses.Commends
{
    public class SetPrimaryWarehouseCommand : IRequest<WarehouseDto>
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
    }
}
