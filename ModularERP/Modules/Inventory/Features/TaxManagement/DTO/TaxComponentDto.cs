namespace ModularERP.Modules.Inventory.Features.TaxManagement.DTO
{
    public class TaxComponentDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string RateType { get; set; } = string.Empty;
        public decimal RateValue { get; set; }
        public string IncludedType { get; set; } = string.Empty;
        public string AppliesOn { get; set; } = string.Empty;
        public bool Active { get; set; }
        public Guid TenantId { get; set; }
    }
}
