namespace ModularERP.Modules.Finance.Features.IncomesVoucher.DTO
{
    namespace ModularERP.Modules.Finance.Features.IncomesVoucher.DTO // أو ExpensesVoucher
    {
        public class TaxLineResponseDto
        {
            public Guid TaxProfileId { get; set; }
            public Guid TaxComponentId { get; set; }
            public string TaxProfileName { get; set; }
            public string TaxComponentName { get; set; }
            public decimal BaseAmount { get; set; }
            public decimal TaxAmount { get; set; }
            public decimal AppliedRate { get; set; }
            public bool IsWithholding { get; set; }
        }
    }
}
