namespace ModularERP.Modules.Inventory.Features.TaxManagement.DTO
{
    public class TaxProfileComponentDto
    {
        public Guid TaxComponentId { get; set; }
        public string ComponentName { get; set; } = string.Empty;
        public string RateType { get; set; } = string.Empty;
        public decimal RateValue { get; set; }
        public string IncludedType { get; set; } = string.Empty;
        public string AppliesOn { get; set; } = string.Empty;
        public int Priority { get; set; }
    }
}
