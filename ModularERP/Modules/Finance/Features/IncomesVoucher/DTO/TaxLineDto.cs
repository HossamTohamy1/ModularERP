using System.ComponentModel.DataAnnotations;

namespace ModularERP.Modules.Finance.Features.IncomesVoucher.DTO
{
    using System.ComponentModel.DataAnnotations;

    namespace ModularERP.Modules.Finance.Features.IncomesVoucher.DTO // أو ExpensesVoucher
    {
        public class TaxLineDto
        {
            [Required]
            public Guid TaxProfileId { get; set; }

            [Required]
            public Guid TaxComponentId { get; set; }

            [Required]
            [Range(0, double.MaxValue)]
            public decimal BaseAmount { get; set; }

            [Required]
            [Range(0, double.MaxValue)]
            public decimal TaxAmount { get; set; }

            [Required]
            [Range(0, 100)]
            public decimal AppliedRate { get; set; }

            public bool IsWithholding { get; set; } = false;
        }
    }
}
