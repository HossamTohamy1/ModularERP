using System.ComponentModel.DataAnnotations;

namespace ModularERP.Modules.Finance.Features.ExpensesVoucher.DTO
{
    public class CreateExpenseVoucherDto
    {
        public string? Code { get; set; }

        [Required]
        public DateTime Date { get; set; }

        public string CurrencyCode { get; set; } = "EGP";
        public decimal FxRate { get; set; } = 1.0m;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required]
        public string Description { get; set; } = string.Empty;

        public CounterpartyDto? Counterparty { get; set; }

        [Required]
        public Guid CategoryId { get; set; }

        [Required]
        public WalletDto Source { get; set; } = new();

        public List<TaxLineDto> TaxLines { get; set; } = new();
        public List<string> Attachments { get; set; } = new();
        public Guid? RecurrenceId { get; set; }
    }
}
