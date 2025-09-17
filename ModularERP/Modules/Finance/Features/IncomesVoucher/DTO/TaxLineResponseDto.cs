namespace ModularERP.Modules.Finance.Features.IncomesVoucher.DTO
{
    public class TaxLineResponseDto
    {
        public Guid TaxId { get; set; }
        public decimal BaseAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public bool IsWithholding { get; set; }
    }
}
