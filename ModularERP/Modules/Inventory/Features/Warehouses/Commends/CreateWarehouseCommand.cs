using MediatR;
using ModularERP.Modules.Inventory.Features.Warehouses.DTO;

namespace ModularERP.Modules.Inventory.Features.Warehouses.Commends
{
    public class CreateWarehouseCommand : IRequest<WarehouseDto>
    {
        public string Name { get; set; }
        public string? ShippingAddress { get; set; }
        public string Status { get; set; }
        public bool IsPrimary { get; set; }
        public Guid CompanyId { get; set; }
    }
}
