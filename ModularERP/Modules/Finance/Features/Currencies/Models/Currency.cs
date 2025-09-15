using ModularERP.Common.Models;
using ModularERP.Modules.Finance.Features.BankAccounts.Models;
using ModularERP.Modules.Finance.Features.LedgerEntries.Models;
using ModularERP.Modules.Finance.Features.Treasuries.Models;
using ModularERP.Modules.Finance.Features.Vouchers.Models;
using System.ComponentModel.DataAnnotations;

namespace ModularERP.Modules.Finance.Features.Currencies.Models
{
    public class Currency : BaseEntity
    {
        [Key, MaxLength(3)]
        public string Code { get; set; } = string.Empty;

        [Required, MaxLength(5)]
        public string Symbol { get; set; } = string.Empty;

        public int Decimals { get; set; } = 2;

        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; }

        // Navigation properties
        public virtual ICollection<Voucher> Vouchers { get; set; } = new List<Voucher>();
        public virtual ICollection<Treasury> Treasuries { get; set; } = new List<Treasury>();
        public virtual ICollection<BankAccount> BankAccounts { get; set; } = new List<BankAccount>();
        public virtual ICollection<LedgerEntry> LedgerEntries { get; set; } = new List<LedgerEntry>();
    }
}
