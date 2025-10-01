namespace ModularERP.Modules.Inventory.Features.Warehouses.DTO
{
    public class CreateWarehouseDto
    {
        public string Name { get; set; }
        public string? ShippingAddress { get; set; }
        public string Status { get; set; } = "Active";
        public bool IsPrimary { get; set; } = false;
        public Guid CompanyId { get; set; }
    }
}
