namespace ModularERP.Modules.Inventory.Features.Warehouses.DTO
{
    public class WarehouseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? ShippingAddress { get; set; }
        public string Status { get; set; }
        public bool IsPrimary { get; set; }
        public Guid CompanyId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
