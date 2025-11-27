namespace ModularERP.Modules.Purchases.Payment.DTO.DTO_PaymentMethod
{
    public class PaymentMethodDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public bool IsActive { get; set; }
        public string? Description { get; set; }
        public bool RequiresReference { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
