using MediatR;
using ModularERP.Modules.Inventory.Features.Warehouses.DTO;

namespace ModularERP.Modules.Inventory.Features.Warehouses.Commends
{
    public class UpdateWarehouseStatusCommand : IRequest<WarehouseDto>
    {
        public Guid Id { get; set; }
        public string Status { get; set; }
        public Guid CompanyId { get; set; }
    }
}
