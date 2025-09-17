using System.ComponentModel.DataAnnotations;

namespace ModularERP.Modules.Finance.Features.ExpensesVoucher.DTO
{
    public class WalletDto
    {
        [Required]
        public string Type { get; set; } = string.Empty; // Treasury, BankAccount

        [Required]
        public Guid Id { get; set; }
    }
}
