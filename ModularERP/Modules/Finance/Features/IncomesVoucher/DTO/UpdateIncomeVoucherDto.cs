using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace ModularERP.Modules.Finance.Features.IncomesVoucher.DTO
{
    public class UpdateIncomeVoucherDto
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public DateTime Date { get; set; }

        public string CurrencyCode { get; set; } = "EGP";
        public decimal FxRate { get; set; } = 1.0m;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public Guid CategoryId { get; set; }

        public Guid? RecurrenceId { get; set; }

        // JSON strings - stored as strings only
        [Required]
        public string SourceJson { get; set; } = string.Empty;

        public string? CounterpartyJson { get; set; }

        public string? TaxLinesJson { get; set; }

        public List<IFormFile> NewAttachments { get; set; } = new();
        public List<Guid>? AttachmentsToRemove { get; set; } = new();

        // For business logic validation - separate fields
        public Guid? SourceId { get; set; }
        public string? SourceType { get; set; }

        // Parsed objects for validation and response
        public WalletDto Source { get; set; } = new WalletDto();

        // Counterparty property
        [System.Text.Json.Serialization.JsonIgnore]
        public CounterpartyDto? Counterparty
        {
            get
            {
                if (string.IsNullOrEmpty(CounterpartyJson))
                    return null;
                try
                {
                    return JsonSerializer.Deserialize<CounterpartyDto>(CounterpartyJson);
                }
                catch
                {
                    return null;
                }
            }
        }
    }
}

