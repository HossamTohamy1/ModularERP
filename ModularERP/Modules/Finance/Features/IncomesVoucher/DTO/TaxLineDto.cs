using System.ComponentModel.DataAnnotations;

namespace ModularERP.Modules.Finance.Features.IncomesVoucher.DTO
{
    public class TaxLineDto
    {
        [Required]
        public Guid TaxId { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal BaseAmount { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal TaxAmount { get; set; }

        public bool IsWithholding { get; set; } = false;
    }
}
