using ModularERP.Modules.Finance.Features.ExpensesVoucher.DTO.ModularERP.Modules.Finance.Features.IncomesVoucher.DTO;

namespace ModularERP.Modules.Finance.Features.ExpensesVoucher.DTO
{
    public class ExpenseVoucherResponseDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public decimal FxRate { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        // ✅ Add Source and Counterparty like Income Voucher
        public WalletDto Source { get; set; } = new WalletDto();
        public CounterpartyDto? Counterparty { get; set; }

        public List<TaxLineResponseDto> TaxLines { get; set; } = new();
        public List<AttachmentResponseDto> Attachments { get; set; } = new();
    }
}