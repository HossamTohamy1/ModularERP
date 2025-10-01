namespace ModularERP.Modules.Inventory.Features.Warehouses.DTO
{
    public class UpdateWarehouseDto
    {
        public string Name { get; set; }
        public string? ShippingAddress { get; set; }
        public string Status { get; set; }
        public bool IsPrimary { get; set; }
        public Guid CompanyId { get; set; }   

    }
}
