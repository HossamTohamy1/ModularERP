namespace ModularERP.Modules.Purchases.Invoicing.DTO.DTO_PaymentApplication
{
    public class VoidAllocationDto
    {
        public Guid AllocationId { get; set; }
        public string VoidReason { get; set; } = string.Empty;
    }
}
