namespace ModularERP.Modules.Purchases.Payment.DTO.DTO_PaymentMethod
{
    public class CreatePaymentMethodDto
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string? Description { get; set; }
        public bool RequiresReference { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
