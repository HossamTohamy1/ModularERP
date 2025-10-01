namespace ModularERP.Modules.Inventory.Features.Warehouses.DTO
{
    public class WarehouseListDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? ShippingAddress { get; set; }
        public string Status { get; set; }
        public bool IsPrimary { get; set; }
    }
}
